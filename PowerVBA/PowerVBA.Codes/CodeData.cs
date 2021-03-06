﻿using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerVBA.Codes
{
    public struct CodeData
    {
        #region [  Basis  ]

        /// <summary>
        /// 첫째 줄인지 여부를 가져옵니다.
        /// </summary>
        public bool IsFistNonWs { get; set; }

        /// <summary>
        /// 주석 여부를 가져옵니다.
        /// </summary>
        public bool IsInComment { get; set; }

        /// <summary>
        /// string 내부 인지를 가져옵니다.
        /// </summary>
        public bool IsInString { get; set; }
        
        /// <summary>
        /// 그대로의 String("와 같은) 인지에 대한 여부를 가져옵니다.
        /// </summary>
        public bool IsInVerbatimString { get; set; }
        
        /// <summary>
        /// 전처리기 지시문인지에 대한 여부를 가져옵니다.
        /// </summary>
        public bool IsInPreprocessorDirective { get; set; }

        #endregion

        
        /// <summary>
        /// 현재 변수 선언 중인지 여부를 나타냅니다. 확실치 않을때도 false를 반환합니다.
        /// </summary>
        public bool IsVarDeclaring { get; set; }

        /// <summary>
        /// 현재 상수 선언 중인지 여부를 나타냅니다. 확실치 않을때도 false를 반환합니다.
        /// </summary>
        public bool IsConstDeclaring { get; set; }

        /// <summary>
        /// 현재 Function 선언 중인지 여부를 나타냅니다. 확실치 않을때도 false를 반환합니다.
        /// </summary>
        public bool IsFuncDeclaring { get; set; }

        /// <summary>
        /// 현재 Sub 선언 중인지 여부를 나타냅니다. 확실치 않을때도 false를 반환합니다.
        /// </summary>
        public bool IsSubDeclaring { get; set; }

        /// <summary>
        /// Public이나 Private 같은 엑세서 이후인지에 대한 여부를 가져옵니다.
        /// </summary>
        public bool AfterAccessor { get; set; }
        
        /// <summary>
        /// Sub나 Function 같은 선언자 이후인지에 대한 여부를 가져옵니다.
        /// </summary>
        public bool AfterDeclarator { get; set; }
        public bool AfterFunction { get; set; }
        public bool AfterSub { get; set; }
        public bool AfterEnum { get; set; }
        public bool AfterArray { get; set; }
        public bool AfterProperty { get; set; }

        #region [  Do Until/While  ]

        public bool AfterDo { get; set; }
        public bool AfterUntil { get; set; }
        public bool AfterWhile { get; set; }
        public bool AfterLoop { get; set; }

        #endregion
        
        /// <summary>
        /// 식별자 이후인지에 대한 여부를 가져옵니다. 이 이후에는 보통 아무것도 없거나, As 키워드가 나와야 합니다.
        /// </summary>
        public bool AfterIdentifier { get; set; }

        #region [  For  ]

        /// <summary>
        /// For 이후인지에 대한 여부를 가져옵니다. 이 이후에는 식별자가 나와야 합니다.
        /// </summary>
        public bool AfterFor { get; set; }
        /// <summary>
        /// For Each 이후인지에 대한 여부를 가져옵니다. 이 이후에는 식별자가 나와야 합니다.
        /// </summary>
        public bool AfterForEach { get; set; }

        #endregion

        /// <summary>
        /// 괄호 안인지에 대한 여부입니다.
        /// </summary>
        public bool IsInBracket { get; set; }

        #region [  If  ]

        /// <summary>
        /// If문이 전부 완성되었는지에 대한 여부를 가져옵니다.
        /// </summary>
        public bool AfterIfProcessing { get => AfterIf && AfterThen; }

        /// <summary>
        /// If 절 이후인지에 대한 여부를 가져옵니다. 이후에는 Expression이 나와야 합니다.
        /// </summary>
        public bool AfterIf { get;  set; }
        /// <summary>
        /// ElseIf 절 이후인지에 대한 여부를 가져옵니다. 이후에는 Expression이 나와야 합니다.
        /// </summary>
        public bool AfterElseIf { get; set; }

        public bool AfterThen { get; set; }

        #endregion

        #region [  Select Case  ]

        /// <summary>
        /// Select 절 이후인지에 대한 여부를 가져옵니다.
        /// </summary>
        public bool AfterSelect { get; set; }
        public bool AfterCase { get; set; }

        #endregion
        
        #region [  Keyword  ]

        /// <summary>
        /// Else 절 이후인지에 대한 여부를 가져옵니다. 이후에는 아무것도 없거나, 
        /// If 한줄 절일때는 Statement를 사용해야 합니다.
        /// </summary>
        public bool AfterElse { get; set; }

        public bool AfterExit { get; set; }

        /// <summary>
        /// As 이후인지에 대한 여부를 가져옵니다. 이 이후에는 타입이 나와야 합니다.
        /// </summary>
        public bool AfterAs { get; set; }


        /// <summary>
        /// End 절 이후인지를 가져옵니다 이 이후에는 아무것도 없거나 
        /// Sub, Function, Type, If, Select등이 나와야 합니다.
        /// </summary>
        public bool AfterEnd { get; set; }


        #endregion
        
        
        public object AfterWend { get; set; }

        #region [  Property  ]

        public bool AfterLet { get; set; }
        public bool AfterSet { get; set; }
        public bool AfterGet { get; set; }

        #endregion

        public bool AfterType { get; set; }
        public bool AfterPropAccessor { get; set; }
        public bool AfterReturn { get; set; }
        public bool UseMultiLine { get; set; }
        
        public bool AfterLabel { get; set; }
        public bool ReadMember { get; set; }
        public bool AfterCallFunction { get; set; }
        public bool AfterExpression { get; set; }
        public bool AfterDeclare { get; set; }


        public bool AfterOnErrorResumeNext { get => AfterOn && AfterError && AfterResume && AfterNext; }
        public bool AfterOnErrorGotoLabel { get => AfterOn && AfterError && AfterGoto && AfterLabel; }
        public bool AfterOn { get;  set; }
        public bool AfterError { get; set; }
        public bool AfterGoto { get;  set; }
        public bool AfterResume { get;  set; }
        public bool AfterNext { get;  set; }
        public bool AfterIn { get;  set; }
        public bool AfterReDim { get; internal set; }
        public bool AfterLib { get; internal set; }
        public bool AfterAlias { get; internal set; }
        public bool AfterString { get; internal set; }
        public bool AfterWith { get; internal set; }

        public bool AfterType_KW { get; internal set; }

        #region [  Option Private Module  ]
        public bool AfterOptionPrivateModule { get => AfterOption && AfterPrivate && AfterModule; }

        public bool AfterOption { get; internal set; }
        public bool AfterPrivate { get; internal set; }
        public bool AfterModule { get; internal set; }
        public bool AfterCompare { get; internal set; }
        public bool AfterBase { get; internal set; }
        public bool AfterExplicit { get; internal set; }
        public bool AfterText { get; internal set; }
        public bool AfterBinary { get; internal set; }
        #endregion
    }
}