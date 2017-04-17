﻿using PowerVBA.Core.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerVBA.Core.Interface;
using PowerVBA.V2013.Wrap.WrapClass;
using Microsoft.Office.Core;
using static PowerVBA.Core.Extension.BoolEx;
using VBA = Microsoft.Vbe.Interop;
using PowerVBA.Core.Wrap.WrapBase;
using Microsoft.Office.Interop.PowerPoint;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using PowerVBA.Global.RegexExpressions;
using System.Diagnostics;

namespace PowerVBA.V2013.Connector
{
    public class PPTConnector2013 : PPTConnectorBase
    {

        internal Thread EventConnectThread;


        private PPTConnector2013()
        {
            Application = new ApplicationWrapping(new Microsoft.Office.Interop.PowerPoint.Application());
            
            EventConnectThread = new Thread(() =>
            {
                int LastComponentCount = 0;
                int LastShapeCount = 0;
                int LastSlideCount = 0;
                
                int DelayCounter = 0;

                while (true)
                {
                    // PPT 종료 확인
                    try
                    {
                        bool Contain = Application.Presentations.Cast<Presentation>().Where((i) => (i.Application.Build == Presentation.Application.Build)).Where((i) => i.Name == Presentation.Name).Count() >= 1;


                        if (Contain) Console.WriteLine("Application에 포함되어 있습니다.");

                        if (VBProject.VBComponents.Count != LastComponentCount)
                        {
                            LastComponentCount = VBProject.VBComponents.Count;
                            OnVBAComponentChange();
                        }

                        DelayCounter++;

                        if (DelayCounter > 4)
                        {
                            // 슬라이드 변경 인식
                            List<SlideWrapping> Slides = new List<SlideWrapping>();
                            Presentation.Slides.Cast<Slide>().ToList().ForEach((i) => Slides.Add(new SlideWrapping(i)));

                            if (Slides.Count != LastSlideCount)
                            {
                                LastSlideCount = Slides.Count;
                                OnSlideChanged();
                            }

                            // 도형 변경 인식
                            int TempShapeCount = 0;

                            Presentation.Slides.Cast<Slide>().Select((i) => new SlideWrapping(i)).ToList()
                                                                                                 .ForEach((i) =>
                            {
                                TempShapeCount += i.Shapes.Count;
                            });

                            if (TempShapeCount != LastShapeCount)
                            {
                                LastShapeCount = TempShapeCount;
                                OnShapeChanged();
                            }


                            DelayCounter = 0;
                        }

                        //OnSectionChanged();

                    }
                    catch (Exception e)
                    {
                        // 발생하는 예외 상황 : Modelless 창이 떴을 경우
                        (int, string)[] Errors = { (-2147417846, "응용 프로그램이 사용 중입니다."),
                                                   // 발생하는 예외 상황 : 프레젠테이션 창이 하나는 있지만 현재 연결되어 있던 PowerPoint 창이 닫혔을때
                                                   (-2147188720, "오브젝트가 존재하지 않습니다."),
                                                   // 발생하는 예외 상황 : 연결된 PowerPoint 창이 닫히면서 프레제텐이션 창 자체가 닫혔을때
                                                   (-2146827864, "애플리케이션이 종료된 것 같습니다."),
                                                   (-2147467262, "애플리케이션이 종료된 것 같습니다.")};
                                                    
                        //
                        var Error = Errors.Where((i) => i.Item1 == e.HResult).FirstOrDefault();

                        if (Error.Item2 != null)
                        {
                            Console.WriteLine($"\"{Error.Item2}\" 라는 알려진 예외가 발생했습니다.");
                            if (Error.Equals(Errors[1]) || Error.Equals(Errors[2])) OnPPTClosed();
                        }
                        else
                        {
                            MessageBox.Show($"알려지지 않은 예외가 발생했습니다. ({e.HResult})" + Environment.NewLine + Environment.NewLine + e.ToString());
                        }
                    }

                    Thread.Sleep(500);
                }
            });
        }
        public PPTConnector2013(string FileLocation, bool NewFile = false, bool OpenWithWindow = true) : this()
        {
            Presentation = new PresentationWrapping(Application.Presentations.Open(FileLocation, MsoTriState.msoFalse, NewFile.BoolToState(), OpenWithWindow.BoolToState()));
            VBProject = new VBProjectWrapping(Presentation.VBProject);


            EventConnectThread?.Start();
        }
        public PPTConnector2013(PresentationWrapping ppt) : this()
        {
            Presentation = ppt;
            VBProject = new VBProjectWrapping(Presentation.VBProject);


            EventConnectThread?.Start();
        }
        public PPTConnector2013(bool OpenWithWindow = true) : this()
        {
            Presentation = new PresentationWrapping(Application.Presentations.Add(OpenWithWindow.BoolToState()));
            VBProject = new VBProjectWrapping(Presentation.VBProject);
            Presentation.Slides.AddSlide(1, Presentation.SlideMaster.CustomLayouts[1]);
            //compWrap.CodeModule.DeleteLines()
            
            EventConnectThread?.Start();
        }

        public PresentationWrapping Presentation { get => (PresentationWrapping)_Presentation; set => _Presentation = value; }
        public ApplicationWrapping Application { get => (ApplicationWrapping)_PPTApp; set => _PPTApp = value; }
        public VBProjectWrapping VBProject { get => (VBProjectWrapping)_VBProject; set => _VBProject = value; }

        public override PPTVersion Version => PPTVersion.PPT2013;

        public override string Name => Presentation.Name;

        public override int SlideCount { get => Presentation.Slides.Count; }

        public bool IsContainsName(string name)
        {
            foreach (VBA.VBComponent comp in VBProject.VBComponents)
            {
                if (name == comp.Name) return true;
            }

            return false;
        }

        #region [  Class/Form/Module 추가/제거  ]

        public override bool AddClass(string name, out VBComponentWrappingBase vbcomp)
        {
            return AddComponent(name, out vbcomp, VBA.vbext_ComponentType.vbext_ct_ClassModule);
        }

        public override bool AddForm(string name, out VBComponentWrappingBase vbcomp)
        {
            return AddComponent(name, out vbcomp, VBA.vbext_ComponentType.vbext_ct_MSForm);
        }

        public override bool AddModule(string name, out VBComponentWrappingBase vbcomp)
        {
            return AddComponent(name, out vbcomp, VBA.vbext_ComponentType.vbext_ct_StdModule);
        }

        private bool AddComponent(string name, out VBComponentWrappingBase vbcomp, VBA.vbext_ComponentType type)
        {
            vbcomp = null;
            if (!Regex.IsMatch(name, CodePattern.ComponentNameRule)) return false;
            if (IsContainsName(name)) return false;
            VBComponentWrapping newStandardModule = null;
            try
            {
                newStandardModule = new VBComponentWrapping(VBProject.VBComponents.Add(type));

                newStandardModule.Name = name;
                vbcomp = newStandardModule;
            }
            catch (Exception)
            {
                if (newStandardModule != null) VBProject.VBComponents.Remove(newStandardModule.VBComponent);
                return false;
            }

            return true;
        }

        public override bool DeleteClass(string name)
        {
            if (!Regex.IsMatch(name, RegexPattern.Pattern.NamePattern)) return false;

            VBProject.VBComponents.Remove(VBProject.VBComponents.Cast<VBA.VBComponent>().Where((i) => (i.Name == name && i.Type == VBA.vbext_ComponentType.vbext_ct_ClassModule)).First());

            return true;
        }

        public override bool DeleteForm(string name)
        {
            if (!Regex.IsMatch(name, RegexPattern.Pattern.NamePattern)) return false;

            VBProject.VBComponents.Remove(VBProject.VBComponents.Cast<VBA.VBComponent>().Where((i) => (i.Name == name && i.Type == VBA.vbext_ComponentType.vbext_ct_MSForm)).First());
            return true;
        }

        public override bool DeleteModule(string name)
        {
            if (!Regex.IsMatch(name, RegexPattern.Pattern.NamePattern)) return false;

            VBProject.VBComponents.Remove(VBProject.VBComponents.Cast<VBA.VBComponent>().Where((i) => (i.Name == name && i.Type == VBA.vbext_ComponentType.vbext_ct_StdModule)).First());

            return true;
        }


        public override bool DeleteComponent(VBComponentWrappingBase comp)
        {
            try
            {
                VBA.VBComponent itm = ((VBComponentWrapping)comp).VBComponent;

                VBProject.VBComponents.Remove(itm);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region [  Class/Module 코드 관리  ]



        #endregion

        #region [  Slide 추가/제거  ]

        public override bool AddSlide()
        {
            int SlideNumber = 0;

            if (Presentation.Slides.Count != 0) SlideNumber = Presentation.Application.ActiveWindow.Selection.SlideRange.SlideIndex;
            try
            {
                Presentation.Slides.AddSlide(SlideNumber + 1, Presentation.Presentation.SlideMaster.CustomLayouts[1]);
                Presentation.Application.ActiveWindow.View.GotoSlide(SlideNumber + 1);
            }
            catch (Exception) { return false; }


            return true;
        }

        public override bool AddSlide(int SlideNumber)
        {

            if (Presentation.Presentation.Slides.Count == 0) return false;

            try
            {
                Presentation.Slides.AddSlide(SlideNumber, Presentation.Presentation.SlideMaster.CustomLayouts[1]);
                Presentation.Application.ActiveWindow.View.GotoSlide(SlideNumber + 1);
            }
            catch (Exception) { return false; }


            return true;
        }

        public override bool DeleteSlide()
        {
            try
            {
                Presentation.Slides[Presentation.Application.ActiveWindow.Selection.SlideRange.SlideIndex].Delete();
            }
            catch (Exception) { return false; }


            return true;
        }

        public override bool DeleteSlide(int SlideNumber)
        {
            try
            {
                Presentation.Slides[SlideNumber].Delete();
            }
            catch (Exception) { return false; }


            return true;
        }

        #endregion



        public override List<ShapeWrappingBase> Shapes()
        {
            List<ShapeWrappingBase> shapes = new List<ShapeWrappingBase>();
            Presentation.Slides.Cast<Slide>()
                               .ToList()
                               .ForEach(i =>
                                   shapes.AddRange(i.Shapes.Cast<Microsoft.Office.Interop.PowerPoint.Shape>()
                                                           .ToList()
                                                           .Select((s) => new ShapeWrapping(s))));

            return shapes;
        }


        public override List<ShapeWrappingBase> Shapes(int Slide)
        {
            List<ShapeWrappingBase> shapes = Presentation.Slides[Slide].Shapes
                                                                       .Cast<Microsoft.Office.Interop.PowerPoint.Shape>()
                                                                       .Select(i => new ShapeWrapping(i))
                                                                       .Cast<ShapeWrappingBase>().ToList();
            
            return shapes;
        }



        public override void Dispose()
        {
            Presentation.Close();
            if (Application.Presentations.Count == 0) Application.Quit();
        }

        #region [  Module/Class/Form 존재 여부 확인  ]
        public override bool ContainsModule(string name)
        {
            return VBProject.VBComponents.Cast<VBA.VBComponent>()
                                         .Where(i => i.Name == name && i.Type == VBA.vbext_ComponentType.vbext_ct_StdModule)
                                         .Count() > 0;
        }

        public override bool ContainsClass(string name)
        {
            return VBProject.VBComponents.Cast<VBA.VBComponent>()
                                         .Where(i => i.Name == name && i.Type == VBA.vbext_ComponentType.vbext_ct_ClassModule)
                                         .Count() > 0;
        }

        public override bool ContainsForm(string name)
        {
            return VBProject.VBComponents.Cast<VBA.VBComponent>()
                                         .Where(i => i.Name == name && i.Type == VBA.vbext_ComponentType.vbext_ct_MSForm)
                                         .Count() > 0;
        }

        public override VBComponentWrappingBase GetModule(string name)
        {
            if (ContainsModule(name))
                return new VBComponentWrapping(VBProject.VBComponents.Cast<VBA.VBComponent>()
                                                                     .Where(i => i.Name == name && i.Type == VBA.vbext_ComponentType.vbext_ct_StdModule).First());

            return null;
        }

        public override VBComponentWrappingBase GetClass(string name)
        {
            if (ContainsModule(name))
                return new VBComponentWrapping(VBProject.VBComponents.Cast<VBA.VBComponent>()
                                                                     .Where(i => i.Name == name && i.Type == VBA.vbext_ComponentType.vbext_ct_ClassModule).First());

            return null;
        }

        public override VBComponentWrappingBase GetFrm(string name)
        {
            if (ContainsModule(name))
                return new VBComponentWrapping(VBProject.VBComponents.Cast<VBA.VBComponent>()
                                                                     .Where(i => i.Name == name && i.Type == VBA.vbext_ComponentType.vbext_ct_MSForm).First());

            return null;
        }

        public override bool Save()
        {
            if (Presentation.ReadOnly == MsoTriState.msoTrue) return false;
            try
            {
                Presentation.Save();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            
        }

        public override bool SaveAs(string path)
        {
            try
            {
                Presentation.SaveAs(path);
                return true;
            }
            catch (Exception)
            { return false; }   
        }

        public override bool AddReference(string Path)
        {
            try
            {
                VBProject.References.AddFromFile(Path);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        #endregion


    }
}
