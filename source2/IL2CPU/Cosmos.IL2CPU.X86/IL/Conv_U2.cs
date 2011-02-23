using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	/// <summary>
	/// Convert top Stack element to UInt16 and extends it to Int32.
	/// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_U2 )]
    public class Conv_U2 : ILOp
    {
        public Conv_U2( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
			var xSource = Assembler.Stack.Pop();
            if (xSource.IsFloat)
            {
                new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
                new CPUx86.SSE.ConvertSS2SI { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            }

            switch( xSource.Size )
            {
                case 1:// this case not sure
                case 4:
					// nothing todo     
					break;
                case 8:
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }
                case 2:
                    {
                        break;
                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U2: SourceSize " + xSource + " not yet supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException();
            }
#if DOTNETCOMPATIBLE
			Assembler.Stack.Push(4, typeof(ushort));
#else
			Assembler.Stack.Push(2, typeof(ushort));
#endif
        }
    }
}