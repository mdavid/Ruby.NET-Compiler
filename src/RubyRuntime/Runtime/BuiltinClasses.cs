using System;
using System.Collections.Generic;
using System.Text;

namespace Ruby.Runtime
{
    class BuiltinClasses
    {
        public static bool IsBuiltinClass(string name)
        {
            switch (name)
            {
                case "ArgumentError":
                case "Array":
                case "Basic":
                case "Bignum":
                case "Binding":
                case "CLRException":
                case "Class":
                case "Comparable":
                case "Continuation":
                case "Crypt":
                case "Data":
                case "Dictionary":
                case "Dir":
                case "EOFError":
                case "Enumerable":
                case "Env":
                case "Errno":
                case "Exception":
                case "FalseClass":
                case "File":
                case "FileStat":
                case "FileTest":
                case "Fixnum":
                case "Float":
                case "FloatDomainError":
                case "GC":
                case "Hash":
                case "IContext":
                case "IO":
                case "IOError":
                case "IndexError":
                case "Integer":
                case "Interrupt":
                case "Kernel":
                case "LoadError":
                case "LocalJumpError":
                case "Marshal":
                case "Match":
                case "MatchData":
                case "Math":
                case "Method":
                case "Module":
                case "NameError":
                case "NilClass":
                case "NoMemoryError":
                case "NoMethodError":
                case "NotImplementedError":
                case "Numeric":
                case "Object":
                case "ObjectSpace":
                case "Pack":
                case "Precision":
                case "Proc":
                case "ProcKind":
                case "Process":
                case "ProcessStatus":
                case "Range":
                case "RangeError":
                case "RegExpError":
                case "Regexp":
                case "RuntimeError":
                case "ScriptError":
                case "SecurityError":
                case "Signal":
                case "SignalException":
                case "Sprintf":
                case "StandardError":
                case "String":
                case "Struct":
                case "Symbol":
                case "SyntaxError":
                case "SystemCallError":
                case "SystemExit":
                case "SystemStackError":
                case "Thread":
                case "ThreadError":
                case "ThreadGroup":
                case "Time":
                case "TrueClass":
                case "TypeError":
                case "UnboundMethod":
                case "ZeroDivisionError":
                    return true;
                default:
                    return false;
            }
        }
    }
}
