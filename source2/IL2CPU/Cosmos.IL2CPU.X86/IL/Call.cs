using System;
using Cosmos.IL2CPU.ILOpCodes;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// 
// using IL2CPU=Cosmos.IL2CPU;
using CPU = Cosmos.Compiler.Assembler.X86;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.IL2CPU.X86;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Cosmos.Compiler.Assembler;
// using System.Reflection;
// using Cosmos.IL2CPU.X86;
// using Cosmos.IL2CPU.Compiler;

namespace Cosmos.IL2CPU.X86.IL {
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Call)]
  public class Call: ILOp {
    //         private string LabelName;
    //         private uint mResultSize;
    //         private uint? TotalArgumentSize = null;
    //         private bool mIsDebugger_Break = false;
    //         private uint[] ArgumentSizes = new uint[0];
    //         private MethodInformation mMethodInfo;
    //         private MethodInformation mTargetMethodInfo;
    //         private string mNextLabelName;
    //         private uint mCurrentILOffset;
    //         private MethodBase mMethod;

    public Call(Cosmos.Compiler.Assembler.Assembler aAsmblr)
      : base(aAsmblr) {
    }

    public static uint GetStackSizeToReservate(MethodBase aMethod) {
      
      var xMethodInfo = aMethod as System.Reflection.MethodInfo;
      uint xReturnSize = 0;
      if (xMethodInfo != null) {
        xReturnSize = SizeOfType(xMethodInfo.ReturnType);
      }
      if (xReturnSize == 0) {
        return 0;
      }
      // todo: implement exception support
      int xExtraStackSize = (int)Align(xReturnSize, 4);
      var xParameters = aMethod.GetParameters();
      foreach (var xItem in xParameters) {
        xExtraStackSize -= (int)Align(SizeOfType(xItem.ParameterType), 4);
      }
      if (!xMethodInfo.IsStatic) {
        xExtraStackSize -= (int)Align(SizeOfType(xMethodInfo.DeclaringType), 4);
      }
      if (xExtraStackSize > 0) {
        return (uint)xExtraStackSize;
      }
      return 0;
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xOpMethod = aOpCode as OpMethod;
      DoExecute(Assembler, aMethod, xOpMethod.Value, xOpMethod.ValueUID, aOpCode, MethodInfoLabelGenerator.GenerateLabelName(aMethod.MethodBase));
    }

    public static void DoExecute(Assembler Assembler, MethodInfo aCurrentMethod, MethodBase aTargetMethod, uint aTargetMethodUID, ILOpCode aCurrent, string currentLabel) {
      //if (aTargetMethod.IsVirtual) {
      //  Callvirt.DoExecute(Assembler, aCurrentMethod, aTargetMethod, aTargetMethodUID, aCurrentPosition);
      //  return;
      //}
      var xMethodInfo = aTargetMethod as System.Reflection.MethodInfo;

      // mTargetMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(mMethod
      //   , mMethod, mMethodDescription, null, mCurrentMethodInfo.DebugMode);
      string xNormalAddress = "";
      if (aTargetMethod.IsStatic || !aTargetMethod.IsVirtual || aTargetMethod.IsFinal) {
        xNormalAddress = MethodInfoLabelGenerator.GenerateLabelName(aTargetMethod);
      } else {
          xNormalAddress = MethodInfoLabelGenerator.GenerateLabelName(aTargetMethod);
        //throw new Exception("Call: non-concrete method called!");
      }
      var xParameters = aTargetMethod.GetParameters();
      int xArgCount = xParameters.Length;
      // todo: implement exception support
      uint xExtraStackSize = GetStackSizeToReservate(aTargetMethod);
      if (xExtraStackSize > 0) {
        new CPUx86.Sub {
          DestinationReg = CPUx86.Registers.ESP,
          SourceValue = (uint)xExtraStackSize
        };
      }
      new CPUx86.Call {
        DestinationLabel = xNormalAddress
      };

      uint xReturnSize=0;
      if (xMethodInfo != null)
      {
          xReturnSize = SizeOfType(xMethodInfo.ReturnType);
      }
      if (aCurrentMethod != null)
      {
          EmitExceptionLogic(Assembler, aCurrentMethod, aCurrent, true,
                     delegate()
                     {
                         var xResultSize = xReturnSize;
                         if (xResultSize % 4 != 0)
                         {
                             xResultSize += 4 - (xResultSize % 4);
                         }
                         for (int i = 0; i < xResultSize / 4; i++)
                         {
                             new CPUx86.Add
                             {
                                 DestinationReg = CPUx86.Registers.ESP,
                                 SourceValue = 4
                             };
                         }
                     });
      }






      //EmitExceptionLogic(Assembler,
      //           mCurrentILOffset,
      //           mMethodInfo,
      //           mNextLabelName,
      //           true,
      //           delegate() {
      //             var xResultSize = mTargetMethodInfo.ReturnSize;
      //             if (xResultSize % 4 != 0) {
      //               xResultSize += 4 - (xResultSize % 4);
      //             }
      //             for (int i = 0; i < xResultSize / 4; i++) {
      //               new CPUx86.Add {
      //                 DestinationReg = CPUx86.Registers.ESP,
      //                 SourceValue = 4
      //               };
      //             }
      //           });
      for (int i = 0; i < xParameters.Length; i++) {
        Assembler.Stack.Pop();
      }
      if (!aTargetMethod.IsStatic) {
        Assembler.Stack.Pop();
      }
      if (xMethodInfo == null || SizeOfType(xMethodInfo.ReturnType) == 0) {
        return;
	  }

#if DOTNETCOMPATIBLE
      Assembler.Stack.Push(ILOp.Align(SizeOfType(xMethodInfo.ReturnType), 4),
                              xMethodInfo.ReturnType);
#else
	  Assembler.Stack.Push(SizeOfType(xMethodInfo.ReturnType),
                              xMethodInfo.ReturnType);
#endif
    }
  }
}