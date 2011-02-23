﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Debug {
    // Messages from Guest to Host
    public enum MsgType: byte { 
        Noop = 0
        , TracePoint = 1
        , Message = 2
        , BreakPoint = 3
        , Error = 4
        , Pointer = 5
        // This is sent once on start up. The first call to debug stub sends this. 
        // Host can then respond with a series of set breakpoints etc, ie ones that were set before running.
        , Started = 6
        , MethodContext = 7
        , MemoryData = 8
        // Sent after commands to acknowledge receipt during batch mode
        , CmdCompleted = 9
    }
    
    // Messages from Host to Guest
    public enum Command : byte {
        Noop = 0
        , TraceOff = 1, TraceOn = 2
        , Break = 3
        , Continue = 4 // After a Break
        , Step = 5
        , BreakOnAddress = 6
        , BatchBegin = 7 // Not used yet
        , BatchEnd = 8 // Not used yet
        , SendMethodContext = 9 // sends data from stack, relative to EBP (in x86)
        , SendMemory = 10
        // Make sure this is always the last entry. Used by DebugStub to verify commands
        , Max = 11
    }
}