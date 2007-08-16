/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Methods;
using Ruby;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Ruby.Runtime;

    // Ruby.Inits - defines standard Ruby classes and modules and binds their methods

namespace Ruby.Runtime
{

    [UsedByRubyCompiler]
    public class Init
    {
        // Classes ...

        [UsedByRubyCompiler]
        public static Class rb_cObject;

        internal static Class rb_cModule;
        internal static Class rb_cClass;
        internal static Class rb_cNilClass;
        internal static Class rb_cSymbol;
        internal static Class rb_cData;
        internal static Class rb_cTrueClass;
        internal static Class rb_cFalseClass;
        internal static Class rb_cString;
        internal static Class rb_cNameErrorMesg;
        internal static Class rb_cThread;
        internal static Class rb_cCont;
        internal static Class rb_cNumeric;
        internal static Class rb_cInteger;
        internal static Class rb_cFixnum;
        internal static Class rb_cFloat;
        internal static Class rb_cBignum;
        internal static Class rb_cArray;
        internal static Class rb_cHash;
        internal static Class rb_cStruct;
        internal static Class rb_cRegexp;
        internal static Class rb_cMatch;
        internal static Class rb_cRange;
        internal static Class rb_cIO;
        internal static Class rb_cFile;
        internal static Class rb_cStat;
        internal static Class rb_cDir;
        internal static Class rb_cTime;
        internal static Class rb_cProcStatus;
        internal static Class rb_cProc;
        internal static Class rb_cMethod;
        internal static Class rb_cUnboundMethod;
        internal static Class rb_cBinding;
        internal static Class cThGroup;

        // Modules ...
        internal static Class rb_mMarshal;
        internal static Class rb_mKernel;
        internal static Class rb_mComparable;
        internal static Class rb_mEnumerable;
        internal static Class rb_mPrecision;
        internal static Class rb_mErrno;
        internal static Class rb_mFileTest;
        internal static Class rb_mFConst;
        internal static Class rb_mProcess;
        internal static Class rb_mProcUID;
        internal static Class rb_mProcGID;
        internal static Class rb_mProcID_Syscall;
        internal static Class rb_mMath;
        internal static Class rb_mObjectSpace;
        internal static Class rb_mGC;
        internal static Class rb_mSignal;

        // Exceptions ...
        internal static Class rb_eException;
        internal static Class rb_eSystemExit;
        internal static Class rb_eFatal;
        internal static Class rb_eSignal;
        internal static Class rb_eInterrupt;
        internal static Class rb_eStandardError;
        internal static Class rb_eTypeError;
        public static Class rb_eArgError;
        internal static Class rb_eIndexError;
        internal static Class rb_eRangeError;
        internal static Class rb_eNameError;
        internal static Class rb_eNoMethodError;
        internal static Class rb_eScriptError;
        internal static Class rb_eSyntaxError;
        internal static Class rb_eLoadError;
        internal static Class rb_eNotImpError;
        internal static Class rb_eRuntimeError;
        internal static Class rb_eSecurityError;
        internal static Class rb_eNoMemError;
        internal static Class rb_eSystemCallError;
        internal static Class rb_eThreadError;
        internal static Class rb_eZeroDivError;
        internal static Class rb_eFloatDomainError;
        internal static Class rb_eRegexpError;
        internal static Class rb_eIOError;
        internal static Class rb_eEOFError;
        internal static Class rb_eLocalJumpError;
        internal static Class rb_eSysStackError;
        internal static Class rb_eCLRException;

        internal static Class S_Tms;

        [UsedByRubyCompiler]
        static Init()
        {
            System.AppDomain.CurrentDomain.AssemblyResolve += Ruby.Compiler.CodeGenContext.ResolveAssembly;

            Init_sym();
            Init_var_tables();
            Init_Object();
            Init_Comparable();
            Init_Enumerable();
            Init_Precision();
            Init_eval();
            Init_String();
            Init_Exception();
            Init_Thread();
            Init_Numeric();
            Init_Bignum();
            Init_syserr();
            Init_Array();
            Init_Hash();
            Init_Struct();
            Init_Regexp();
            Init_pack();
            Init_Range();
            Init_IO();
            Init_Dir();
            Init_Time();
            Init_Random();
            Init_signal();
            Init_process();
            Init_load();
            Init_Proc();
            Init_Binding();
            Init_Math();
            Init_GC();
            Init_marshal();
            Init_version();

            Init_Prog();

            // BBTAG: need to set default visibility to private
            rb_cObject.scope_vmode = Access.Private;
            Eval.RubyRunning = true;
            //System.Console.WriteLine("------------------------------------------------");


        }

        static void Init_sym()
        {
            //sym_tbl = st_init_strtable_with_size(200);
            //sym_rev_tbl = st_init_numtable_with_size(200);
        }

        static void Init_var_tables()
        {
            //rb_global_tbl = st_init_numtable();
            //rb_class_tbl = st_init_numtable();
            //autoload = rb_intern("__autoload__");
            //classpath = rb_intern("__classpath__");
            //tmp_classpath = rb_intern("__tmp_classpath__");
        }


        static void Init_Object()
        {
            Class metaclass;

            rb_cObject = Class.boot_defclass("Object", null);
            rb_cModule = Class.boot_defclass("Module", rb_cObject);
            rb_cClass = Class.boot_defclass("Class", rb_cModule);

            metaclass = Class.rb_make_metaclass(rb_cObject, rb_cClass);
            metaclass = Class.rb_make_metaclass(rb_cModule, metaclass);
            metaclass = Class.rb_make_metaclass(rb_cClass, metaclass);

            rb_mKernel = Class.rb_define_module("Kernel", null);
            Class.rb_include_module(null, rb_cObject, rb_mKernel);
            Class.rb_define_alloc_func(rb_cObject, rb_class_allocate_instance.singleton);
            Class.rb_define_private_method(rb_cObject, "initialize", rb_obj_dummy.singleton, 0, null);
            Class.rb_define_private_method(rb_cClass, "inherited", rb_obj_dummy.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "included", rb_obj_dummy.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "extended", rb_obj_dummy.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "method_added", rb_obj_dummy.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "method_removed", rb_obj_dummy.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "method_undefined", rb_obj_dummy.singleton, 1, null);

            Class.rb_define_method(rb_mKernel, "nil?", rb_false.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "==", rb_obj_equal.singleton, 1, null);
            Class.rb_define_method(rb_mKernel, "equal?", rb_obj_equal.singleton, 1, null);
            Class.rb_define_method(rb_mKernel, "===", rb_equal.singleton, 1, null);
            Class.rb_define_method(rb_mKernel, "=~", rb_obj_pattern_match.singleton, 1, null);

            Class.rb_define_method(rb_mKernel, "eql?", rb_obj_equal.singleton, 1, null);

            Class.rb_define_method(rb_mKernel, "hash", rb_obj_id.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "id", rb_obj_id_obsolete.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "__id__", rb_obj_id.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "object_id", rb_obj_id.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "type", rb_obj_type.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "class", rb_obj_class.singleton, 0, null);

            Class.rb_define_method(rb_mKernel, "clone", rb_obj_clone.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "dup", rb_obj_dup.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "initialize_copy", rb_obj_init_copy.singleton, 1, null);

            Class.rb_define_method(rb_mKernel, "taint", rb_obj_taint.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "tainted?", rb_obj_tainted.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "untaint", rb_obj_untaint.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "freeze", rb_obj_freeze.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "frozen?", rb_obj_frozen_p.singleton, 0, null);

            Class.rb_define_method(rb_mKernel, "to_a", rb_any_to_a.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "to_s", rb_any_to_s.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "inspect", rb_obj_inspect.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "methods", rb_obj_methods.singleton, -1, null);
            Class.rb_define_method(rb_mKernel, "singleton_methods", rb_obj_singleton_methods.singleton, -1, null);
            Class.rb_define_method(rb_mKernel, "protected_methods", rb_obj_protected_methods.singleton, -1, null);
            Class.rb_define_method(rb_mKernel, "private_methods", rb_obj_private_methods.singleton, -1, null);
            Class.rb_define_method(rb_mKernel, "public_methods", rb_obj_public_methods.singleton, -1, null);
            Class.rb_define_method(rb_mKernel, "instance_variables", rb_obj_instance_variables.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "instance_variable_get", rb_obj_ivar_get.singleton, 1, null);
            Class.rb_define_method(rb_mKernel, "instance_variable_set", rb_obj_ivar_set.singleton, 2, null);
            Class.rb_define_private_method(rb_mKernel, "remove_instance_variable", rb_obj_remove_instance_variable.singleton, 1, null);

            Class.rb_define_method(rb_mKernel, "instance_of?", rb_obj_is_instance_of.singleton, 1, null);
            Class.rb_define_method(rb_mKernel, "kind_of?", rb_obj_is_kind_of.singleton, 1, null);
            Class.rb_define_method(rb_mKernel, "is_a?", rb_obj_is_kind_of.singleton, 1, null);

            Class.rb_define_private_method(rb_mKernel, "singleton_method_added", rb_obj_dummy.singleton, 1, null);
            Class.rb_define_private_method(rb_mKernel, "singleton_method_removed", rb_obj_dummy.singleton, 1, null);
            Class.rb_define_private_method(rb_mKernel, "singleton_method_undefined", rb_obj_dummy.singleton, 1, null);

            Class.rb_define_global_function("sprintf", rb_f_sprintf.singleton, -1, null);
            Class.rb_define_global_function("format", rb_f_sprintf.singleton, -1, null);

            Class.rb_define_global_function("Integer", rb_f_integer.singleton, 1, null);
            Class.rb_define_global_function("Float", rb_f_float.singleton, 1, null);

            Class.rb_define_global_function("String", rb_f_string.singleton, 1, null);
            Class.rb_define_global_function("Array", rb_f_array.singleton, 1, null);

            rb_cNilClass = Class.rb_define_class("NilClass", rb_cObject, null);
            Class.rb_define_method(rb_cNilClass, "to_i", nil_to_i.singleton, 0, null);
            Class.rb_define_method(rb_cNilClass, "to_f", nil_to_f.singleton, 0, null);
            Class.rb_define_method(rb_cNilClass, "to_s", nil_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cNilClass, "to_a", nil_to_a.singleton, 0, null);
            Class.rb_define_method(rb_cNilClass, "inspect", nil_inspect.singleton, 0, null);
            Class.rb_define_method(rb_cNilClass, "&", false_and.singleton, 1, null);
            Class.rb_define_method(rb_cNilClass, "|", false_or.singleton, 1, null);
            Class.rb_define_method(rb_cNilClass, "^", false_xor.singleton, 1, null);

            Class.rb_define_method(rb_cNilClass, "nil?", rb_true.singleton, 0, null);
            Class.rb_undef_alloc_func(rb_cNilClass);
            Class.rb_undef_method(Class.CLASS_OF(rb_cNilClass), "new");
            Variables.rb_define_global_const("NIL", null);

            rb_cSymbol = Class.rb_define_class("Symbol", rb_cObject, null);
            Class.rb_define_singleton_method(rb_cSymbol, "all_symbols", rb_sym_all_symbols.singleton, 0, null);
            Class.rb_undef_alloc_func(rb_cSymbol);
            Class.rb_undef_method(Class.CLASS_OF(rb_cSymbol), "new");

            Class.rb_define_method(rb_cSymbol, "to_i", sym_to_i.singleton, 0, null);
            Class.rb_define_method(rb_cSymbol, "to_int", sym_to_int.singleton, 0, null);
            Class.rb_define_method(rb_cSymbol, "inspect", sym_inspect.singleton, 0, null);
            Class.rb_define_method(rb_cSymbol, "to_s", sym_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cSymbol, "id2name", sym_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cSymbol, "to_sym", sym_to_sym.singleton, 0, null);
            Class.rb_define_method(rb_cSymbol, "eql?", rb_sym_eql.singleton, 1, null);
            Class.rb_define_method(rb_cSymbol, "===", rb_sym_eql.singleton, 1, null);

            Class.rb_define_method(rb_cModule, "freeze", rb_mod_freeze.singleton, 0, null);
            Class.rb_define_method(rb_cModule, "===", rb_mod_eqq.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "==", rb_obj_equal.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "<=>", rb_mod_cmp.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "<", rb_mod_lt.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "<=", rb_class_inherited_p.singleton, 1, null);
            Class.rb_define_method(rb_cModule, ">", rb_mod_gt.singleton, 1, null);
            Class.rb_define_method(rb_cModule, ">=", rb_mod_ge.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "initialize_copy", rb_mod_init_copy.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "to_s", rb_mod_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cModule, "included_modules", rb_mod_included_modules.singleton, 0, null);
            Class.rb_define_method(rb_cModule, "include?", rb_mod_include_p.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "name", rb_mod_name.singleton, 0, null);
            Class.rb_define_method(rb_cModule, "ancestors", rb_mod_ancestors.singleton, 0, null);

            Class.rb_define_private_method(rb_cModule, "attr", rb_mod_attr.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "attr_reader", rb_mod_attr_reader.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "attr_writer", rb_mod_attr_writer.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "attr_accessor", rb_mod_attr_accessor.singleton, 1, null);

            Class.rb_define_alloc_func(rb_cModule, rb_module_s_alloc.singleton);
            Class.rb_define_method(rb_cModule, "initialize", rb_mod_initialize.singleton, 0, null);
            Class.rb_define_method(rb_cModule, "instance_methods", rb_class_instance_methods.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "public_instance_methods", rb_class_public_instance_methods.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "protected_instance_methods", rb_class_protected_instance_methods.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "private_instance_methods", rb_class_private_instance_methods.singleton, 1, null);

            Class.rb_define_method(rb_cModule, "constants", rb_mod_constants.singleton, 0, null);
            Class.rb_define_method(rb_cModule, "const_get", rb_mod_const_get.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "const_set", rb_mod_const_set.singleton, 2, null);
            Class.rb_define_method(rb_cModule, "const_defined?", rb_mod_const_defined.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "remove_const", rb_mod_remove_const.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "const_missing", rb_mod_const_missing.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "class_variables", rb_mod_class_variables.singleton, 0, null);
            Class.rb_define_private_method(rb_cModule, "remove_class_variable", rb_mod_remove_cvar.singleton, 1, null);
            Class.rb_define_method(rb_cClass, "allocate", rb_obj_alloc.singleton, 0, null);
            Class.rb_define_method(rb_cClass, "new", rb_class_new_instance.singleton, 1, null);
            Class.rb_define_method(rb_cClass, "initialize", rb_class_initialize.singleton, 1, null);
            Class.rb_define_method(rb_cClass, "initialize_copy", rb_class_init_copy.singleton, 1, null);
            Class.rb_define_method(rb_cClass, "superclass", rb_class_superclass.singleton, 0, null);
            Class.rb_define_alloc_func(rb_cClass, rb_class_s_alloc.singleton);
            Class.rb_undef_method(rb_cClass, "extend_object");
            Class.rb_undef_method(rb_cClass, "append_features");

            rb_cData = Class.rb_define_class("Data", rb_cObject, null);
            Class.rb_undef_alloc_func(rb_cData);

            Object.ruby_top_self = new Object();
            //rb_global_variable(Object.ruby_top_self);
            Class.rb_define_singleton_method(Object.ruby_top_self, "to_s", main_to_s.singleton, 0, null);

            rb_cTrueClass = Class.rb_define_class("TrueClass", rb_cObject, null);
            Class.rb_define_method(rb_cTrueClass, "to_s", true_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cTrueClass, "&", true_and.singleton, 1, null);
            Class.rb_define_method(rb_cTrueClass, "|", true_or.singleton, 1, null);
            Class.rb_define_method(rb_cTrueClass, "^", true_xor.singleton, 1, null);
            Class.rb_undef_alloc_func(rb_cTrueClass);
            Class.rb_undef_method(Class.CLASS_OF(rb_cTrueClass), "new");
            Variables.rb_define_global_const("TRUE", true);

            rb_cFalseClass = Class.rb_define_class("FalseClass", rb_cObject, null);
            Class.rb_define_method(rb_cFalseClass, "to_s", false_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cFalseClass, "&", false_and.singleton, 1, null);
            Class.rb_define_method(rb_cFalseClass, "|", false_or.singleton, 1, null);
            Class.rb_define_method(rb_cFalseClass, "^", false_xor.singleton, 1, null);
            Class.rb_undef_alloc_func(rb_cFalseClass);
            Class.rb_undef_method(Class.CLASS_OF(rb_cFalseClass), "new");
            Variables.rb_define_global_const("FALSE", false);

            //id_eq = rb_intern("==");
            //id_eql = rb_intern("eql?");
            //id_inspect = rb_intern("inspect");
            //id_init_copy = rb_intern("initialize_copy");
        }

        static void Init_Comparable()
        {
            rb_mComparable = Class.rb_define_module("Comparable", null);
            Class.rb_define_method(rb_mComparable, "==", cmp_equal.singleton, 1, null);
            Class.rb_define_method(rb_mComparable, ">", cmp_gt.singleton, 1, null);
            Class.rb_define_method(rb_mComparable, ">=", cmp_ge.singleton, 1, null);
            Class.rb_define_method(rb_mComparable, "<", cmp_lt.singleton, 1, null);
            Class.rb_define_method(rb_mComparable, "<=", cmp_le.singleton, 1, null);
            Class.rb_define_method(rb_mComparable, "between?", cmp_between.singleton, 2, null);

            //cmp = rb_intern("<=>");
        }

        static void Init_Enumerable()
        {
            rb_mEnumerable = Class.rb_define_module("Enumerable", null);

            Class.rb_define_method(rb_mEnumerable, "to_a", enum_to_a.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "entries", enum_to_a.singleton, 0, null);

            Class.rb_define_method(rb_mEnumerable, "sort", enum_sort.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "sort_by", enum_sort_by.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "grep", enum_grep.singleton, 1, null);
            Class.rb_define_method(rb_mEnumerable, "find", enum_find.singleton, -1, null);
            Class.rb_define_method(rb_mEnumerable, "detect", enum_find.singleton, -1, null);
            Class.rb_define_method(rb_mEnumerable, "find_all", enum_find_all.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "select", enum_find_all.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "reject", enum_reject.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "collect", enum_collect.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "map", enum_collect.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "inject", enum_inject.singleton, -1, null);
            Class.rb_define_method(rb_mEnumerable, "partition", enum_partition.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "all?", enum_all.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "any?", enum_any.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "min", enum_min.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "max", enum_max.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "member?", enum_member.singleton, 1, null);
            Class.rb_define_method(rb_mEnumerable, "include?", enum_member.singleton, 1, null);
            Class.rb_define_method(rb_mEnumerable, "each_with_index", enum_each_with_index.singleton, 0, null);
            Class.rb_define_method(rb_mEnumerable, "zip", enum_zip.singleton, -1, null);

            //id_eqq  = rb_intern("===");
            //id_each = rb_intern("each");
            //id_cmp  = rb_intern("<=>");
        }

        static void Init_Precision()
        {
            rb_mPrecision = Class.rb_define_module("Precision", null);
            Class.rb_define_singleton_method(rb_mPrecision, "included", prec_included.singleton, 1, null);
            Class.rb_define_method(rb_mPrecision, "prec", prec_prec.singleton, 1, null);
            Class.rb_define_method(rb_mPrecision, "prec_i", prec_prec_i.singleton, 0, null);
            Class.rb_define_method(rb_mPrecision, "prec_f", prec_prec_f.singleton, 0, null);

            //prc_pr = rb_intern("prec");
            //prc_if = rb_intern("induced_from");
        }

        static void Init_eval()
        {
            //init = rb_intern("initialize");
            //eqq = rb_intern("===");
            //each = rb_intern("each");

            //aref = rb_intern("[]");
            //aset = rb_intern("[]=");
            //match = rb_intern("=~");
            //missing = rb_intern("method_missing");
            //added = rb_intern("method_added");
            //singleton_added = rb_intern("singleton_method_added");
            //removed = rb_intern("method_removed");
            //singleton_removed = rb_intern("singleton_method_removed");
            //undefined = rb_intern("method_undefined");
            //singleton_undefined = rb_intern("singleton_method_undefined");

            //__id__ = rb_intern("__id__");
            //__send__ = rb_intern("__send__");

            //rb_global_variable(Eval.top_scope);
            //rb_global_variable(Eval.ruby_eval_tree_begin);

            //rb_global_variable(Eval.ruby_eval_tree);
            //rb_global_variable(Eval.ruby_dyna_vars);

            Variables.rb_define_variable("$@", Eval.errat);
            Variables.rb_define_variable("$!", Eval.ruby_errinfo);

            Class.rb_define_global_function("eval", rb_f_eval.singleton, -1, null);
            Class.rb_define_global_function("iterator?", rb_f_block_given_p.singleton, 0, null);
            Class.rb_define_global_function("block_given?", rb_f_block_given_p.singleton, 0, null);
            Class.rb_define_global_function("method_missing", rb_method_missing.singleton, -1, null);
            Class.rb_define_global_function("loop", rb_f_loop.singleton, 0, null);

            Class.rb_define_method(rb_mKernel, "respond_to?", rb_obj_respond_to.singleton, -1, null);
            //respond_to   = rb_intern("respond_to?");
            //Eval.basic_respond_to = Eval.rb_method_node(rb_cObject, "respond_to?");
            //rb_global_variable(Eval.basic_respond_to);

            Class.rb_define_global_function("raise", rb_f_raise.singleton, -1, null);
            Class.rb_define_global_function("fail", rb_f_raise.singleton, -1, null);

            Class.rb_define_global_function("caller", rb_f_caller.singleton, -1, null);

            Class.rb_define_global_function("exit", rb_f_exit.singleton, -1, null);
            Class.rb_define_global_function("abort", rb_f_abort.singleton, -1, null);

            Class.rb_define_global_function("at_exit", rb_f_at_exit.singleton, 0, null);

            Class.rb_define_global_function("catch", rb_f_catch.singleton, 1, null);
            Class.rb_define_global_function("throw", rb_f_throw.singleton, -1, null);
            Class.rb_define_global_function("global_variables", rb_f_global_variables.singleton, 0, null);
            Class.rb_define_global_function("local_variables", rb_f_local_variables.singleton, 0, null);

            Class.rb_define_method(rb_mKernel, "send", rb_f_send.singleton, -1, null);
            Class.rb_define_method(rb_mKernel, "__send__", rb_f_send.singleton, -1, null);
            Class.rb_define_method(rb_mKernel, "instance_eval", rb_obj_instance_eval.singleton, -1, null);

            Class.rb_define_private_method(rb_cModule, "append_features", rb_mod_append_features.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "extend_object", rb_mod_extend_object.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "include", rb_mod_include.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "public", rb_mod_public.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "internal", rb_mod_public.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "protected", rb_mod_protected.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "private", rb_mod_private.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "module_function", rb_mod_modfunc.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "method_defined?", rb_mod_method_defined.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "public_method_defined?", rb_mod_public_method_defined.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "private_method_defined?", rb_mod_private_method_defined.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "protected_method_defined?", rb_mod_protected_method_defined.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "public_class_method", rb_mod_public_method.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "private_class_method", rb_mod_private_method.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "module_eval", rb_mod_module_eval.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "class_eval", rb_mod_module_eval.singleton, 1, null);

            Class.rb_undef_method(rb_cClass, "module_function");

            Class.rb_define_private_method(rb_cModule, "remove_method", rb_mod_remove_method.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "undef_method", rb_mod_undef_method.singleton, 1, null);
            Class.rb_define_private_method(rb_cModule, "alias_method", rb_mod_alias_method.singleton, 2, null);
            Class.rb_define_private_method(rb_cModule, "define_method", rb_mod_define_method.singleton, -1, null);

            Class.rb_define_singleton_method(rb_cModule, "nesting", rb_mod_nesting.singleton, 0, null);
            Class.rb_define_singleton_method(rb_cModule, "constants", rb_mod_s_constants.singleton, 0, null);

            Class.rb_define_singleton_method(Object.ruby_top_self, "include", top_include.singleton, -1, null);
            Class.rb_define_singleton_method(Object.ruby_top_self, "internal", top_public.singleton, -1, null);
            Class.rb_define_singleton_method(Object.ruby_top_self, "private", top_private.singleton, -1, null);

            Class.rb_define_method(rb_mKernel, "extend", rb_obj_extend.singleton, -1, null);

            Class.rb_define_global_function("trace_var", rb_f_trace_var.singleton, -1, null);
            Class.rb_define_global_function("untrace_var", rb_f_untrace_var.singleton, -1, null);

            Class.rb_define_global_function("set_trace_func", set_trace_func.singleton, 1, null);
            //rb_global_variable(Eval.trace_func);

            Variables.rb_define_variable("$SAFE", Eval.safe);
        }

        static void Init_String()
        {
            rb_cString = Class.rb_define_class("String", rb_cObject, null);
            Class.rb_include_module(null, rb_cString, rb_mComparable);
            Class.rb_include_module(null, rb_cString, rb_mEnumerable);
            Class.rb_define_alloc_func(rb_cString, str_alloc.singleton);
            Class.rb_define_method(rb_cString, "initialize", rb_str_init.singleton, -1, null);
            Class.rb_define_method(rb_cString, "initialize_copy", rb_str_replace.singleton, 1, null);
            Class.rb_define_method(rb_cString, "<=>", rb_str_cmp_m.singleton, 1, null);
            Class.rb_define_method(rb_cString, "==", rb_str_equal.singleton, 1, null);
            Class.rb_define_method(rb_cString, "eql?", rb_str_eql.singleton, 1, null);
            Class.rb_define_method(rb_cString, "hash", rb_str_hash_m.singleton, 0, null);
            Class.rb_define_method(rb_cString, "casecmp", rb_str_casecmp.singleton, 1, null);
            Class.rb_define_method(rb_cString, "+", rb_str_plus.singleton, 1, null);
            Class.rb_define_method(rb_cString, "*", rb_str_times.singleton, 1, null);
            Class.rb_define_method(rb_cString, "%", rb_str_format.singleton, 1, null);
            Class.rb_define_method(rb_cString, "[]", rb_str_aref_m.singleton, -1, null);
            Class.rb_define_method(rb_cString, "[]=", rb_str_aset_m.singleton, -1, null);
            Class.rb_define_method(rb_cString, "insert", rb_str_insert.singleton, 2, null);
            Class.rb_define_method(rb_cString, "length", rb_str_length.singleton, 0, null);
            Class.rb_define_method(rb_cString, "size", rb_str_length.singleton, 0, null);
            Class.rb_define_method(rb_cString, "empty?", rb_str_empty.singleton, 0, null);
            Class.rb_define_method(rb_cString, "=~", rb_str_match.singleton, 1, null);
            Class.rb_define_method(rb_cString, "match", rb_str_match_m.singleton, 1, null);
            Class.rb_define_method(rb_cString, "succ", rb_str_succ.singleton, 0, null);
            Class.rb_define_method(rb_cString, "succ!", rb_str_succ_bang.singleton, 0, null);
            Class.rb_define_method(rb_cString, "next", rb_str_succ.singleton, 0, null);
            Class.rb_define_method(rb_cString, "next!", rb_str_succ_bang.singleton, 0, null);
            Class.rb_define_method(rb_cString, "upto", rb_str_upto_m.singleton, 1, null);
            Class.rb_define_method(rb_cString, "index", rb_str_index_m.singleton, -1, null);
            Class.rb_define_method(rb_cString, "rindex", rb_str_rindex_m.singleton, -1, null);
            Class.rb_define_method(rb_cString, "replace", rb_str_replace.singleton, 1, null);

            Class.rb_define_method(rb_cString, "to_i", rb_str_to_i.singleton, -1, null);
            Class.rb_define_method(rb_cString, "to_f", rb_str_to_f.singleton, 0, null);
            Class.rb_define_method(rb_cString, "to_s", rb_str_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cString, "to_str", rb_str_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cString, "inspect", rb_str_inspect.singleton, 0, null);
            Class.rb_define_method(rb_cString, "dump", rb_str_dump.singleton, 0, null);

            Class.rb_define_method(rb_cString, "upcase", rb_str_upcase.singleton, 0, null);
            Class.rb_define_method(rb_cString, "downcase", rb_str_downcase.singleton, 0, null);
            Class.rb_define_method(rb_cString, "capitalize", rb_str_capitalize.singleton, 0, null);
            Class.rb_define_method(rb_cString, "swapcase", rb_str_swapcase.singleton, 0, null);

            Class.rb_define_method(rb_cString, "upcase!", rb_str_upcase_bang.singleton, 0, null);
            Class.rb_define_method(rb_cString, "downcase!", rb_str_downcase_bang.singleton, 0, null);
            Class.rb_define_method(rb_cString, "capitalize!", rb_str_capitalize_bang.singleton, 0, null);
            Class.rb_define_method(rb_cString, "swapcase!", rb_str_swapcase_bang.singleton, 0, null);

            Class.rb_define_method(rb_cString, "hex", rb_str_hex.singleton, 0, null);
            Class.rb_define_method(rb_cString, "oct", rb_str_oct.singleton, 0, null);
            Class.rb_define_method(rb_cString, "split", rb_str_split_m.singleton, -1, null);
            Class.rb_define_method(rb_cString, "reverse", rb_str_reverse.singleton, 0, null);
            Class.rb_define_method(rb_cString, "reverse!", rb_str_reverse_bang.singleton, 0, null);
            Class.rb_define_method(rb_cString, "concat", rb_str_concat.singleton, 1, null);
            Class.rb_define_method(rb_cString, "<<", rb_str_concat.singleton, 1, null);
            Class.rb_define_method(rb_cString, "crypt", rb_str_crypt.singleton, 1, null);
            Class.rb_define_method(rb_cString, "intern", rb_str_intern.singleton, 0, null);
            Class.rb_define_method(rb_cString, "to_sym", rb_str_intern.singleton, 0, null);

            Class.rb_define_method(rb_cString, "include?", rb_str_include.singleton, 1, null);

            Class.rb_define_method(rb_cString, "scan", rb_str_scan.singleton, 1, null);

            Class.rb_define_method(rb_cString, "ljust", rb_str_ljust.singleton, -1, null);
            Class.rb_define_method(rb_cString, "rjust", rb_str_rjust.singleton, -1, null);
            Class.rb_define_method(rb_cString, "center", rb_str_center.singleton, -1, null);

            Class.rb_define_method(rb_cString, "sub", rb_str_sub.singleton, -1, null);
            Class.rb_define_method(rb_cString, "gsub", rb_str_gsub.singleton, -1, null);
            Class.rb_define_method(rb_cString, "chop", rb_str_chop.singleton, 0, null);
            Class.rb_define_method(rb_cString, "chomp", rb_str_chomp.singleton, -1, null);
            Class.rb_define_method(rb_cString, "strip", rb_str_strip.singleton, 0, null);
            Class.rb_define_method(rb_cString, "lstrip", rb_str_lstrip.singleton, 0, null);
            Class.rb_define_method(rb_cString, "rstrip", rb_str_rstrip.singleton, 0, null);

            Class.rb_define_method(rb_cString, "sub!", rb_str_sub_bang.singleton, -1, null);
            Class.rb_define_method(rb_cString, "gsub!", rb_str_gsub_bang.singleton, -1, null);
            Class.rb_define_method(rb_cString, "chop!", rb_str_chop_bang.singleton, 0, null);
            Class.rb_define_method(rb_cString, "chomp!", rb_str_chomp_bang.singleton, -1, null);
            Class.rb_define_method(rb_cString, "strip!", rb_str_strip_bang.singleton, 0, null);
            Class.rb_define_method(rb_cString, "lstrip!", rb_str_lstrip_bang.singleton, 0, null);
            Class.rb_define_method(rb_cString, "rstrip!", rb_str_rstrip_bang.singleton, 0, null);

            Class.rb_define_method(rb_cString, "tr", rb_str_tr.singleton, 2, null);
            Class.rb_define_method(rb_cString, "tr_s", rb_str_tr_s.singleton, 2, null);
            Class.rb_define_method(rb_cString, "delete", rb_str_delete.singleton, -1, null);
            Class.rb_define_method(rb_cString, "squeeze", rb_str_squeeze.singleton, -1, null);
            Class.rb_define_method(rb_cString, "count", rb_str_count.singleton, -1, null);

            Class.rb_define_method(rb_cString, "tr!", rb_str_tr_bang.singleton, 2, null);
            Class.rb_define_method(rb_cString, "tr_s!", rb_str_tr_s_bang.singleton, 2, null);
            Class.rb_define_method(rb_cString, "delete!", rb_str_delete_bang.singleton, -1, null);
            Class.rb_define_method(rb_cString, "squeeze!", rb_str_squeeze_bang.singleton, -1, null);

            Class.rb_define_method(rb_cString, "each_line", rb_str_each_line.singleton, -1, null);
            Class.rb_define_method(rb_cString, "each", rb_str_each_line.singleton, -1, null);
            Class.rb_define_method(rb_cString, "each_byte", rb_str_each_byte.singleton, 0, null);

            Class.rb_define_method(rb_cString, "sum", rb_str_sum.singleton, -1, null);

            Class.rb_define_global_function("sub", rb_f_sub.singleton, -1, null);
            Class.rb_define_global_function("gsub", rb_f_gsub.singleton, -1, null);

            Class.rb_define_global_function("sub!", rb_f_sub_bang.singleton, -1, null);
            Class.rb_define_global_function("gsub!", rb_f_gsub_bang.singleton, -1, null);

            Class.rb_define_global_function("chop", rb_f_chop.singleton, 0, null);
            Class.rb_define_global_function("chop!", rb_f_chop_bang.singleton, 0, null);

            Class.rb_define_global_function("chomp", rb_f_chomp.singleton, -1, null);
            Class.rb_define_global_function("chomp!", rb_f_chomp_bang.singleton, -1, null);

            Class.rb_define_global_function("split", rb_f_split.singleton, -1, null);
            Class.rb_define_global_function("scan", rb_f_scan.singleton, 1, null);

            Class.rb_define_method(rb_cString, "slice", rb_str_aref_m.singleton, -1, null);
            Class.rb_define_method(rb_cString, "slice!", rb_str_slice_bang.singleton, -1, null);

            //id_to_s = rb_intern("to_s");

            //String.rb_fs = null;  // BBTAG: why was this set to null?
            Variables.rb_define_variable("$;", String.rb_fs);
            Variables.rb_define_variable("$-F", String.rb_fs);
        }

        static void Init_Exception()
        {
            rb_eException = Class.rb_define_class("Exception", rb_cObject, null);
            Class.rb_define_alloc_func(rb_eException, exc_alloc.singleton);
            Class.rb_define_singleton_method(rb_eException, "exception", rb_class_new_instance.singleton, 1, null);
            Class.rb_define_method(rb_eException, "exception", exc_exception.singleton, 1, null);
            Class.rb_define_method(rb_eException, "initialize", exc_initialize.singleton, 1, null);
            Class.rb_define_method(rb_eException, "to_s", exc_to_s.singleton, 0, null);
            Class.rb_define_method(rb_eException, "to_str", exc_to_str.singleton, 0, null);
            Class.rb_define_method(rb_eException, "message", exc_to_str.singleton, 0, null);
            Class.rb_define_method(rb_eException, "inspect", exc_inspect.singleton, 0, null);
            Class.rb_define_method(rb_eException, "backtrace", exc_backtrace.singleton, 0, null);
            Class.rb_define_method(rb_eException, "set_backtrace", exc_set_backtrace.singleton, 1, null);

            rb_eSystemExit = Class.rb_define_class("SystemExit", rb_eException, null);
            Class.rb_define_alloc_func(rb_eSystemExit, exit_alloc.singleton);
            Class.rb_define_method(rb_eSystemExit, "initialize", exit_initialize.singleton, 1, null);
            Class.rb_define_method(rb_eSystemExit, "status", exit_status.singleton, 0, null);
            Class.rb_define_method(rb_eSystemExit, "success?", exit_success_p.singleton, 0, null);

            rb_eFatal = Class.rb_define_class("fatal", rb_eException, null);
            rb_eSignal = Class.rb_define_class("SignalException", rb_eException, null);
            rb_eInterrupt = Class.rb_define_class("Interrupt", rb_eSignal, null);

            rb_eStandardError = Class.rb_define_class("StandardError", rb_eException, null);
            rb_eTypeError = Class.rb_define_class("TypeError", rb_eStandardError, null);
            rb_eArgError = Class.rb_define_class("ArgumentError", rb_eStandardError, null);
            rb_eIndexError = Class.rb_define_class("IndexError", rb_eStandardError, null);
            rb_eRangeError = Class.rb_define_class("RangeError", rb_eStandardError, null);
            rb_eNameError = Class.rb_define_class("NameError", rb_eStandardError, null);
            Class.rb_define_alloc_func(rb_eNameError, name_err_alloc.singleton);
            Class.rb_define_method(rb_eNameError, "initialize", name_err_initialize.singleton, 1, null);
            Class.rb_define_method(rb_eNameError, "name", name_err_name.singleton, 0, null);
            Class.rb_define_method(rb_eNameError, "to_s", name_err_to_s.singleton, 0, null);
            rb_cNameErrorMesg = Class.rb_define_class_under(rb_eNameError, "message", rb_cData, null);
            Class.rb_define_singleton_method(rb_cNameErrorMesg, "!", name_err_mesg_new.singleton, 3, null);
            Class.rb_define_method(rb_cNameErrorMesg, "to_str", name_err_mesg_to_str.singleton, 0, null);
            Class.rb_define_method(rb_cNameErrorMesg, "_dump", name_err_mesg_to_str.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cNameErrorMesg, "_load", name_err_mesg_load.singleton, 1, null);
            rb_eNoMethodError = Class.rb_define_class("NoMethodError", rb_eNameError, null);
            Class.rb_define_alloc_func(rb_eNoMethodError, nometh_err_alloc.singleton);
            Class.rb_define_method(rb_eNoMethodError, "initialize", nometh_err_initialize.singleton, 1, null);
            Class.rb_define_method(rb_eNoMethodError, "args", nometh_err_args.singleton, 0, null);

            rb_eScriptError = Class.rb_define_class("ScriptError", rb_eException, null);
            rb_eSyntaxError = Class.rb_define_class("SyntaxError", rb_eScriptError, null);
            rb_eLoadError = Class.rb_define_class("LoadError", rb_eScriptError, null);
            rb_eNotImpError = Class.rb_define_class("NotImplementedError", rb_eScriptError, null);

            rb_eRuntimeError = Class.rb_define_class("RuntimeError", rb_eStandardError, null);
            rb_eSecurityError = Class.rb_define_class("SecurityError", rb_eStandardError, null);
            rb_eNoMemError = Class.rb_define_class("NoMemoryError", rb_eException, null);

            rb_eCLRException = Class.rb_define_class("CLRException", rb_eStandardError, null);

            //syserr_tbl = st_init_numtable();

            rb_eSystemCallError = Class.rb_define_class("SystemCallError", rb_eStandardError, null);
            Class.rb_define_alloc_func(rb_eSystemCallError, syserr_alloc.singleton);
            Class.rb_define_method(rb_eSystemCallError, "initialize", syserr_initialize.singleton, 1, null);
            Class.rb_define_method(rb_eSystemCallError, "errno", syserr_errno.singleton, 0, null);
            Class.rb_define_singleton_method(rb_eSystemCallError, "===", syserr_eqq.singleton, 1, null);

            rb_mErrno = Class.rb_define_module("Errno", null);

            Class.rb_define_global_function("warn", rb_warn_m.singleton, 1, null);
        }

        static void Init_Thread()
        {
            rb_eThreadError = Class.rb_define_class("ThreadError", rb_eStandardError, null);
            rb_cThread = Class.rb_define_class("Thread", rb_cObject, null);
            Class.rb_undef_alloc_func(rb_cThread);

            Class.rb_define_singleton_method(rb_cThread, "new", rb_thread_s_new.singleton, 1, null);
            Class.rb_define_method(rb_cThread, "initialize", rb_thread_initialize.singleton, 2, null);
            Class.rb_define_singleton_method(rb_cThread, "start", rb_thread_start.singleton, 2, null);
            Class.rb_define_singleton_method(rb_cThread, "fork", rb_thread_start.singleton, 2, null);

            Class.rb_define_singleton_method(rb_cThread, "stop", rb_thread_stop.singleton, 0, null);
            Class.rb_define_singleton_method(rb_cThread, "kill", rb_thread_s_kill.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cThread, "exit", rb_thread_exit.singleton, 0, null);
            Class.rb_define_singleton_method(rb_cThread, "pass", rb_thread_pass.singleton, 0, null);
            Class.rb_define_singleton_method(rb_cThread, "current", rb_thread_current.singleton, 0, null);
            Class.rb_define_singleton_method(rb_cThread, "main", rb_thread_main.singleton, 0, null);
            Class.rb_define_singleton_method(rb_cThread, "list", rb_thread_list.singleton, 0, null);

            Class.rb_define_singleton_method(rb_cThread, "critical", rb_thread_critical_get.singleton, 0, null);
            Class.rb_define_singleton_method(rb_cThread, "critical=", rb_thread_critical_set.singleton, 1, null);

            Class.rb_define_singleton_method(rb_cThread, "abort_on_exception", rb_thread_s_abort_exc.singleton, 0, null);
            Class.rb_define_singleton_method(rb_cThread, "abort_on_exception=", rb_thread_s_abort_exc_set.singleton, 1, null);

            Class.rb_define_method(rb_cThread, "run", rb_thread_run.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "wakeup", rb_thread_wakeup.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "kill", rb_thread_kill.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "terminate", rb_thread_kill.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "exit", rb_thread_kill.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "value", rb_thread_value.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "status", rb_thread_status.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "join", rb_thread_join_m.singleton, 1, null);
            Class.rb_define_method(rb_cThread, "alive?", rb_thread_alive_p.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "stop?", rb_thread_stop_p.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "raise", rb_thread_raise_m.singleton, 1, null);

            Class.rb_define_method(rb_cThread, "abort_on_exception", rb_thread_abort_exc.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "abort_on_exception=", rb_thread_abort_exc_set.singleton, 1, null);

            Class.rb_define_method(rb_cThread, "priority", rb_thread_priority.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "priority=", rb_thread_priority_set.singleton, 1, null);
            Class.rb_define_method(rb_cThread, "safe_level", rb_thread_safe_level.singleton, 0, null);
            Class.rb_define_method(rb_cThread, "group", rb_thread_group.singleton, 0, null);

            Class.rb_define_method(rb_cThread, "[]", rb_thread_aref.singleton, 1, null);
            Class.rb_define_method(rb_cThread, "[]=", rb_thread_aset.singleton, 2, null);
            Class.rb_define_method(rb_cThread, "key?", rb_thread_key_p.singleton, 1, null);
            Class.rb_define_method(rb_cThread, "keys", rb_thread_keys.singleton, 0, null);

            Class.rb_define_method(rb_cThread, "inspect", rb_thread_inspect.singleton, 0, null);

            rb_cCont = Class.rb_define_class("Continuation", rb_cObject, null);
            Class.rb_undef_alloc_func(rb_cCont);
            Class.rb_undef_method(Class.CLASS_OF(rb_cCont), "new");
            Class.rb_define_method(rb_cCont, "call", rb_cont_call.singleton, 1, null);
            Class.rb_define_method(rb_cCont, "[]", rb_cont_call.singleton, 1, null);
            Class.rb_define_global_function("callcc", rb_callcc.singleton, 0, null);
            //rb_global_variable(Eval.cont_protect);

            cThGroup = Class.rb_define_class("ThreadGroup", rb_cObject, null);
            Class.rb_define_alloc_func(cThGroup, thgroup_s_alloc.singleton);
            Class.rb_define_method(cThGroup, "list", thgroup_list.singleton, 0, null);
            Class.rb_define_method(cThGroup, "enclose", thgroup_enclose.singleton, 0, null);
            Class.rb_define_method(cThGroup, "enclosed?", thgroup_enclosed_p.singleton, 0, null);
            Class.rb_define_method(cThGroup, "add", thgroup_add.singleton, 1, null);
            Eval.thgroup_default = new ThreadGroup();
            Variables.rb_define_const(cThGroup, "Default", Eval.thgroup_default);
            //rb_global_variable(Eval.thgroup_default);

            Eval.curr_thread = Eval.main_thread = new Thread();
        }

        static void Init_Numeric()
        {
            //id_coerce = rb_intern("coerce");
            //id_to_i = rb_intern("to_i");
            //id_eq = rb_intern("==");

            rb_eZeroDivError = Class.rb_define_class("ZeroDivisionError", rb_eStandardError, null);
            rb_eFloatDomainError = Class.rb_define_class("FloatDomainError", rb_eRangeError, null);
            rb_cNumeric = Class.rb_define_class("Numeric", rb_cObject, null);

            Class.rb_define_method(rb_cNumeric, "singleton_method_added", num_sadded.singleton, 1, null);
            Class.rb_include_module(null, rb_cNumeric, rb_mComparable);
            Class.rb_define_method(rb_cNumeric, "initialize_copy", num_init_copy.singleton, 1, null);
            Class.rb_define_method(rb_cNumeric, "coerce", num_coerce.singleton, 1, null);

            Class.rb_define_method(rb_cNumeric, "+@", num_uplus.singleton, 0, null);
            Class.rb_define_method(rb_cNumeric, "-@", num_uminus.singleton, 0, null);
            Class.rb_define_method(rb_cNumeric, "<=>", num_cmp.singleton, 1, null);
            Class.rb_define_method(rb_cNumeric, "eql?", num_eql.singleton, 1, null);
            Class.rb_define_method(rb_cNumeric, "quo", num_quo.singleton, 1, null);
            Class.rb_define_method(rb_cNumeric, "div", num_div.singleton, 1, null);
            Class.rb_define_method(rb_cNumeric, "divmod", num_divmod.singleton, 1, null);
            Class.rb_define_method(rb_cNumeric, "modulo", num_modulo.singleton, 1, null);
            Class.rb_define_method(rb_cNumeric, "remainder", num_remainder.singleton, 1, null);
            Class.rb_define_method(rb_cNumeric, "abs", num_abs.singleton, 0, null);
            Class.rb_define_method(rb_cNumeric, "to_int", num_to_int.singleton, 0, null);

            Class.rb_define_method(rb_cNumeric, "integer?", num_int_p.singleton, 0, null);
            Class.rb_define_method(rb_cNumeric, "zero?", num_zero_p.singleton, 0, null);
            Class.rb_define_method(rb_cNumeric, "nonzero?", num_nonzero_p.singleton, 0, null);

            Class.rb_define_method(rb_cNumeric, "floor", num_floor.singleton, 0, null);
            Class.rb_define_method(rb_cNumeric, "ceil", num_ceil.singleton, 0, null);
            Class.rb_define_method(rb_cNumeric, "round", num_round.singleton, 0, null);
            Class.rb_define_method(rb_cNumeric, "truncate", num_truncate.singleton, 0, null);
            Class.rb_define_method(rb_cNumeric, "step", num_step.singleton, -1, null);

            rb_cInteger = Class.rb_define_class("Integer", rb_cNumeric, null);
            Class.rb_undef_alloc_func(rb_cInteger);
            Class.rb_undef_method(Class.CLASS_OF(rb_cInteger), "new");

            Class.rb_define_method(rb_cInteger, "integer?", int_int_p.singleton, 0, null);
            Class.rb_define_method(rb_cInteger, "upto", int_upto.singleton, 1, null);
            Class.rb_define_method(rb_cInteger, "downto", int_downto.singleton, 1, null);
            Class.rb_define_method(rb_cInteger, "times", int_dotimes.singleton, 0, null);
            Class.rb_include_module(null, rb_cInteger, rb_mPrecision);
            Class.rb_define_method(rb_cInteger, "succ", int_succ.singleton, 0, null);
            Class.rb_define_method(rb_cInteger, "next", int_succ.singleton, 0, null);
            Class.rb_define_method(rb_cInteger, "chr", int_chr.singleton, 0, null);
            Class.rb_define_method(rb_cInteger, "to_i", int_to_i.singleton, 0, null);
            Class.rb_define_method(rb_cInteger, "to_int", int_to_i.singleton, 0, null);
            Class.rb_define_method(rb_cInteger, "floor", int_to_i.singleton, 0, null);
            Class.rb_define_method(rb_cInteger, "ceil", int_to_i.singleton, 0, null);
            Class.rb_define_method(rb_cInteger, "round", int_to_i.singleton, 0, null);
            Class.rb_define_method(rb_cInteger, "truncate", int_to_i.singleton, 0, null);

            rb_cFixnum = Class.rb_define_class("Fixnum", rb_cInteger, null);
            Class.rb_include_module(null, rb_cFixnum, rb_mPrecision);
            Class.rb_define_singleton_method(rb_cFixnum, "induced_from", rb_fix_induced_from.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cInteger, "induced_from", rb_int_induced_from.singleton, 1, null);

            Class.rb_define_method(rb_cFixnum, "to_s", fix_to_s.singleton, -1, null);

            Class.rb_define_method(rb_cFixnum, "id2name", fix_id2name.singleton, 0, null);
            Class.rb_define_method(rb_cFixnum, "to_sym", fix_to_sym.singleton, 0, null);

            Class.rb_define_method(rb_cFixnum, "-@", fix_uminus.singleton, 0, null);
            Class.rb_define_method(rb_cFixnum, "+", fix_plus.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "-", fix_minus.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "*", fix_mul.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "/", fix_div.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "div", fix_div.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "%", fix_mod.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "modulo", fix_mod.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "divmod", fix_divmod.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "quo", fix_quo.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "**", fix_pow.singleton, 1, null);

            Class.rb_define_method(rb_cFixnum, "abs", fix_abs.singleton, 0, null);

            Class.rb_define_method(rb_cFixnum, "==", fix_equal.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "<=>", fix_cmp.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, ">", fix_gt.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, ">=", fix_ge.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "<", fix_lt.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "<=", fix_le.singleton, 1, null);

            Class.rb_define_method(rb_cFixnum, "~", fix_rev.singleton, 0, null);
            Class.rb_define_method(rb_cFixnum, "&", fix_and.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "|", fix_or.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "^", fix_xor.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, "[]", fix_aref.singleton, 1, null);

            Class.rb_define_method(rb_cFixnum, "<<", fix_lshift.singleton, 1, null);
            Class.rb_define_method(rb_cFixnum, ">>", fix_rshift.singleton, 1, null);

            Class.rb_define_method(rb_cFixnum, "to_f", fix_to_f.singleton, 0, null);
            Class.rb_define_method(rb_cFixnum, "size", fix_size.singleton, 0, null);
            Class.rb_define_method(rb_cFixnum, "zero?", fix_zero_p.singleton, 0, null);

            rb_cFloat = Class.rb_define_class("Float", rb_cNumeric, null);

            Class.rb_undef_alloc_func(rb_cFloat);
            Class.rb_undef_method(Class.CLASS_OF(rb_cFloat), "new");

            Class.rb_define_singleton_method(rb_cFloat, "induced_from", rb_flo_induced_from.singleton, 1, null);
            Class.rb_include_module(null, rb_cFloat, rb_mPrecision);

            Variables.rb_define_const(rb_cFloat, "ROUNDS", Numeric.FLT_ROUNDS);
            Variables.rb_define_const(rb_cFloat, "RADIX", Numeric.FLT_RADIX);
            Variables.rb_define_const(rb_cFloat, "MANT_DIG", Numeric.DBL_MANT_DIG);
            Variables.rb_define_const(rb_cFloat, "DIG", Numeric.DBL_DIG);
            Variables.rb_define_const(rb_cFloat, "MIN_EXP", Numeric.DBL_MIN_EXP);
            Variables.rb_define_const(rb_cFloat, "MAX_EXP", Numeric.DBL_MAX_EXP);
            Variables.rb_define_const(rb_cFloat, "MIN_10_EXP", Numeric.DBL_MIN_10_EXP);
            Variables.rb_define_const(rb_cFloat, "MAX_10_EXP", Numeric.DBL_MAX_10_EXP);
            Variables.rb_define_const(rb_cFloat, "MIN", new Float(Numeric.DBL_MIN));
            Variables.rb_define_const(rb_cFloat, "MAX", new Float(Numeric.DBL_MAX));
            Variables.rb_define_const(rb_cFloat, "EPSILON", new Float(Numeric.DBL_EPSILON));

            Class.rb_define_method(rb_cFloat, "to_s", flo_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "coerce", flo_coerce.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "-@", flo_uminus.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "+", flo_plus.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "-", flo_minus.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "*", flo_mul.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "/", flo_div.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "%", flo_mod.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "modulo", flo_mod.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "divmod", flo_divmod.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "**", flo_pow.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "==", flo_eq.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "<=>", flo_cmp.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, ">", flo_gt.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, ">=", flo_ge.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "<", flo_lt.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "<=", flo_le.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "eql?", flo_eql.singleton, 1, null);
            Class.rb_define_method(rb_cFloat, "hash", flo_hash.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "to_f", flo_to_f.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "abs", flo_abs.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "zero?", flo_zero_p.singleton, 0, null);

            Class.rb_define_method(rb_cFloat, "to_i", flo_truncate.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "to_int", flo_truncate.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "floor", flo_floor.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "ceil", flo_ceil.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "round", flo_round.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "truncate", flo_truncate.singleton, 0, null);

            Class.rb_define_method(rb_cFloat, "nan?", flo_is_nan_p.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "infinite?", flo_is_infinite_p.singleton, 0, null);
            Class.rb_define_method(rb_cFloat, "finite?", flo_is_finite_p.singleton, 0, null);
        }

        static void Init_Bignum()
        {
            rb_cBignum = Class.rb_define_class("Bignum", rb_cInteger, null);

            Class.rb_define_method(rb_cBignum, "to_s", rb_big_to_s.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "coerce", rb_big_coerce.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "-@", rb_big_uminus.singleton, 0, null);
            Class.rb_define_method(rb_cBignum, "+", rb_big_plus.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "-", rb_big_minus.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "*", rb_big_mul.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "/", rb_big_div.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "%", rb_big_modulo.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "div", rb_big_div.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "divmod", rb_big_divmod.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "modulo", rb_big_modulo.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "remainder", rb_big_remainder.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "quo", rb_big_quo.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "**", rb_big_pow.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "&", rb_big_and.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "|", rb_big_or.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "^", rb_big_xor.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "~", rb_big_neg.singleton, 0, null);
            Class.rb_define_method(rb_cBignum, "<<", rb_big_lshift.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, ">>", rb_big_rshift.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "[]", rb_big_aref.singleton, 1, null);

            Class.rb_define_method(rb_cBignum, "<=>", rb_big_cmp.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "==", rb_big_eq.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "eql?", rb_big_eql.singleton, 1, null);
            Class.rb_define_method(rb_cBignum, "hash", rb_big_hash.singleton, 0, null);
            Class.rb_define_method(rb_cBignum, "to_f", rb_big_to_f.singleton, 0, null);
            Class.rb_define_method(rb_cBignum, "abs", rb_big_abs.singleton, 0, null);
            Class.rb_define_method(rb_cBignum, "size", rb_big_size.singleton, 0, null);
        }

        static void Init_syserr()
        {
            Errno.set_syserr(Errno.EPERM, "EPERM");
            Errno.set_syserr(Errno.ENOENT, "ENOENT");
            Errno.set_syserr(Errno.ESRCH, "ESRCH");
            Errno.set_syserr(Errno.EINTR, "EINTR");
            Errno.set_syserr(Errno.EIO, "EIO");
            Errno.set_syserr(Errno.ENXIO, "ENXIO");
            Errno.set_syserr(Errno.E2BIG, "E2BIG");
            Errno.set_syserr(Errno.ENOEXEC, "ENOEXEC");
            Errno.set_syserr(Errno.EBADF, "EBADF");
            Errno.set_syserr(Errno.ECHILD, "ECHILD");
            Errno.set_syserr(Errno.EAGAIN, "EAGAIN");
            Errno.set_syserr(Errno.ENOMEM, "ENOMEM");
            Errno.set_syserr(Errno.EACCES, "EACCES");
            Errno.set_syserr(Errno.EFAULT, "EFAULT");
            Errno.set_syserr(Errno.EBUSY, "EBUSY");
            Errno.set_syserr(Errno.EEXIST, "EEXIST");
            Errno.set_syserr(Errno.EXDEV, "EXDEV");
            Errno.set_syserr(Errno.ENODEV, "ENODEV");
            Errno.set_syserr(Errno.ENOTDIR, "ENOTDIR");
            Errno.set_syserr(Errno.EISDIR, "EISDIR");
            Errno.set_syserr(Errno.EINVAL, "EINVAL");
            Errno.set_syserr(Errno.ENFILE, "ENFILE");
            Errno.set_syserr(Errno.EMFILE, "EMFILE");
            Errno.set_syserr(Errno.ENOTTY, "ENOTTY");
            Errno.set_syserr(Errno.EFBIG, "EFBIG");
            Errno.set_syserr(Errno.ENOSPC, "ENOSPC");
            Errno.set_syserr(Errno.ESPIPE, "ESPIPE");
            Errno.set_syserr(Errno.EROFS, "EROFS");
            Errno.set_syserr(Errno.EMLINK, "EMLINK");
            Errno.set_syserr(Errno.EPIPE, "EPIPE");
            Errno.set_syserr(Errno.EDOM, "EDOM");
            Errno.set_syserr(Errno.ERANGE, "ERANGE");
            Errno.set_syserr(Errno.EDEADLK, "EDEADLK");
            Errno.set_syserr(Errno.ENAMETOOLONG, "ENAMETOOLONG");
            Errno.set_syserr(Errno.ENOLCK, "ENOLCK");
            Errno.set_syserr(Errno.ENOSYS, "ENOSYS");
            Errno.set_syserr(Errno.ENOTEMPTY, "ENOTEMPTY");
            Errno.set_syserr(Errno.ELOOP, "ELOOP");
            Errno.set_syserr(Errno.EWOULDBLOCK, "EWOULDBLOCK");
            Errno.set_syserr(Errno.EDEADLOCK, "EDEADLOCK");
            Errno.set_syserr(Errno.EREMOTE, "EREMOTE");
            Errno.set_syserr(Errno.EILSEQ, "EILSEQ");
            Errno.set_syserr(Errno.EUSERS, "EUSERS");
            Errno.set_syserr(Errno.ENOTSOCK, "ENOTSOCK");
            Errno.set_syserr(Errno.EDESTADDRREQ, "EDESTADDRREQ");
            Errno.set_syserr(Errno.EMSGSIZE, "EMSGSIZE");
            Errno.set_syserr(Errno.EPROTOTYPE, "EPROTOTYPE");
            Errno.set_syserr(Errno.ENOPROTOOPT, "ENOPROTOOPT");
            Errno.set_syserr(Errno.EPROTONOSUPPORT, "EPROTONOSUPPORT");
            Errno.set_syserr(Errno.ESOCKTNOSUPPORT, "ESOCKTNOSUPPORT");
            Errno.set_syserr(Errno.EOPNOTSUPP, "EOPNOTSUPP");
            Errno.set_syserr(Errno.EPFNOSUPPORT, "EPFNOSUPPORT");
            Errno.set_syserr(Errno.EAFNOSUPPORT, "EAFNOSUPPORT");
            Errno.set_syserr(Errno.EADDRINUSE, "EADDRINUSE");
            Errno.set_syserr(Errno.EADDRNOTAVAIL, "EADDRNOTAVAIL");
            Errno.set_syserr(Errno.ENETDOWN, "ENETDOWN");
            Errno.set_syserr(Errno.ENETUNREACH, "ENETUNREACH");
            Errno.set_syserr(Errno.ENETRESET, "ENETRESET");
            Errno.set_syserr(Errno.ECONNABORTED, "ECONNABORTED");
            Errno.set_syserr(Errno.ECONNRESET, "ECONNRESET");
            Errno.set_syserr(Errno.ENOBUFS, "ENOBUFS");
            Errno.set_syserr(Errno.EISCONN, "EISCONN");
            Errno.set_syserr(Errno.ENOTCONN, "ENOTCONN");
            Errno.set_syserr(Errno.ESHUTDOWN, "ESHUTDOWN");
            Errno.set_syserr(Errno.ETOOMANYREFS, "ETOOMANYREFS");
            Errno.set_syserr(Errno.ETIMEDOUT, "ETIMEDOUT");
            Errno.set_syserr(Errno.ECONNREFUSED, "ECONNREFUSED");
            Errno.set_syserr(Errno.EHOSTDOWN, "EHOSTDOWN");
            Errno.set_syserr(Errno.EHOSTUNREACH, "EHOSTUNREACH");
            Errno.set_syserr(Errno.EALREADY, "EALREADY");
            Errno.set_syserr(Errno.EINPROGRESS, "EINPROGRESS");
            Errno.set_syserr(Errno.ESTALE, "ESTALE");
            Errno.set_syserr(Errno.EDQUOT, "EDQUOT");
        }

        static void Init_Array()
        {
            rb_cArray = Class.rb_define_class("Array", rb_cObject, null);
            Class.rb_include_module(null, rb_cArray, rb_mEnumerable);

            Class.rb_define_alloc_func(rb_cArray, ary_alloc.singleton);
            Class.rb_define_singleton_method(rb_cArray, "[]", rb_ary_s_create.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "initialize", rb_ary_initialize.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "initialize_copy", rb_ary_replace.singleton, 1, null);

            Class.rb_define_method(rb_cArray, "to_s", rb_ary_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "inspect", rb_ary_inspect.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "to_a", rb_ary_to_a.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "to_ary", rb_ary_to_ary_m.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "frozen?", rb_ary_frozen_p.singleton, 0, null);

            Class.rb_define_method(rb_cArray, "==", rb_ary_equal.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "eql?", rb_ary_eql.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "hash", rb_ary_hash.singleton, 0, null);

            Class.rb_define_method(rb_cArray, "[]", rb_ary_aref.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "[]=", rb_ary_aset.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "at", rb_ary_at.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "fetch", rb_ary_fetch.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "first", rb_ary_first.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "last", rb_ary_last.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "concat", rb_ary_concat.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "<<", rb_ary_push.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "push", rb_ary_push_m.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "pop", rb_ary_pop.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "shift", rb_ary_shift.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "unshift", rb_ary_unshift_m.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "insert", rb_ary_insert.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "each", rb_ary_each.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "each_index", rb_ary_each_index.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "reverse_each", rb_ary_reverse_each.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "length", rb_ary_length.singleton, 0, null);
            Class.rb_define_alias(rb_cArray, "size", "length", null);
            Class.rb_define_method(rb_cArray, "empty?", rb_ary_empty_p.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "index", rb_ary_index.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "rindex", rb_ary_rindex.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "indexes", rb_ary_indexes.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "indices", rb_ary_indexes.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "join", rb_ary_join_m.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "reverse", rb_ary_reverse_m.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "reverse!", rb_ary_reverse_bang.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "sort", rb_ary_sort.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "sort!", rb_ary_sort_bang.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "collect", rb_ary_collect.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "collect!", rb_ary_collect_bang.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "map", rb_ary_collect.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "map!", rb_ary_collect_bang.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "select", rb_ary_select.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "values_at", rb_ary_values_at.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "delete", rb_ary_delete.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "delete_at", rb_ary_delete_at_m.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "delete_if", rb_ary_delete_if.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "reject", rb_ary_reject.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "reject!", rb_ary_reject_bang.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "zip", rb_ary_zip.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "transpose", rb_ary_transpose.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "replace", rb_ary_replace.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "clear", rb_ary_clear.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "fill", rb_ary_fill.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "include?", rb_ary_includes.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "<=>", rb_ary_cmp.singleton, 1, null);

            Class.rb_define_method(rb_cArray, "slice", rb_ary_aref.singleton, -1, null);
            Class.rb_define_method(rb_cArray, "slice!", rb_ary_slice_bang.singleton, -1, null);

            Class.rb_define_method(rb_cArray, "assoc", rb_ary_assoc.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "rassoc", rb_ary_rassoc.singleton, 1, null);

            Class.rb_define_method(rb_cArray, "+", rb_ary_plus.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "*", rb_ary_times.singleton, 1, null);

            Class.rb_define_method(rb_cArray, "-", rb_ary_diff.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "&", rb_ary_and.singleton, 1, null);
            Class.rb_define_method(rb_cArray, "|", rb_ary_or.singleton, 1, null);

            Class.rb_define_method(rb_cArray, "uniq", rb_ary_uniq.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "uniq!", rb_ary_uniq_bang.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "compact", rb_ary_compact.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "compact!", rb_ary_compact_bang.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "flatten", rb_ary_flatten.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "flatten!", rb_ary_flatten_bang.singleton, 0, null);
            Class.rb_define_method(rb_cArray, "nitems", rb_ary_nitems.singleton, 0, null);

            //id_cmp = rb_intern("<=>");
            //inspect_key = rb_intern("__inspect_key__");
        }

        static void Init_Hash()
        {
            //id_hash = rb_intern("hash");
            //id_call = rb_intern("call");
            //id_default = rb_intern("default");

            rb_cHash = Class.rb_define_class("Hash", rb_cObject, null);

            Class.rb_include_module(null, rb_cHash, rb_mEnumerable);

            Class.rb_define_alloc_func(rb_cHash, hash_alloc.singleton);
            Class.rb_define_singleton_method(rb_cHash, "[]", rb_hash_s_create.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "initialize", rb_hash_initialize.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "initialize_copy", rb_hash_replace.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "rehash", rb_hash_rehash.singleton, 0, null);

            Class.rb_define_method(rb_cHash, "to_hash", rb_hash_to_hash.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "to_a", rb_hash_to_a.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "to_s", rb_hash_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "inspect", rb_hash_inspect.singleton, 0, null);

            Class.rb_define_method(rb_cHash, "==", rb_hash_equal.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "[]", rb_hash_aref.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "fetch", rb_hash_fetch.singleton, -1, null);
            Class.rb_define_method(rb_cHash, "[]=", rb_hash_aset.singleton, 2, null);
            Class.rb_define_method(rb_cHash, "store", rb_hash_aset.singleton, 2, null);
            Class.rb_define_method(rb_cHash, "default", rb_hash_default.singleton, -1, null);
            Class.rb_define_method(rb_cHash, "default=", rb_hash_set_default.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "default_proc", rb_hash_default_proc.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "index", rb_hash_index.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "indexes", rb_hash_indexes.singleton, -1, null);
            Class.rb_define_method(rb_cHash, "indices", rb_hash_indexes.singleton, -1, null);
            Class.rb_define_method(rb_cHash, "size", rb_hash_size.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "length", rb_hash_size.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "empty?", rb_hash_empty_p.singleton, 0, null);

            Class.rb_define_method(rb_cHash, "each", rb_hash_each.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "each_value", rb_hash_each_value.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "each_key", rb_hash_each_key.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "each_pair", rb_hash_each_pair.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "sort", rb_hash_sort.singleton, 0, null);

            Class.rb_define_method(rb_cHash, "keys", rb_hash_keys.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "values", rb_hash_values.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "values_at", rb_hash_values_at.singleton, -1, null);

            Class.rb_define_method(rb_cHash, "shift", rb_hash_shift.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "delete", rb_hash_delete.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "delete_if", rb_hash_delete_if.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "select", rb_hash_select.singleton, -1, null);
            Class.rb_define_method(rb_cHash, "reject", rb_hash_reject.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "reject!", rb_hash_reject_bang.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "clear", rb_hash_clear.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "invert", rb_hash_invert.singleton, 0, null);
            Class.rb_define_method(rb_cHash, "update", rb_hash_update.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "replace", rb_hash_replace.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "merge!", rb_hash_update.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "merge", rb_hash_merge.singleton, 1, null);

            Class.rb_define_method(rb_cHash, "include?", rb_hash_has_key.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "member?", rb_hash_has_key.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "has_key?", rb_hash_has_key.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "has_value?", rb_hash_has_value.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "key?", rb_hash_has_key.singleton, 1, null);
            Class.rb_define_method(rb_cHash, "value?", rb_hash_has_value.singleton, 1, null);

            Hash.envtbl = new Object();
            Class.rb_extend_object(null, Hash.envtbl, rb_mEnumerable);

            Class.rb_define_singleton_method(Hash.envtbl, "[]", rb_f_getenv.singleton, 1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "fetch", env_fetch.singleton, -1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "[]=", env_aset.singleton, 2, null);
            Class.rb_define_singleton_method(Hash.envtbl, "store", env_aset.singleton, 2, null);
            Class.rb_define_singleton_method(Hash.envtbl, "each", env_each.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "each_pair", env_each_pair.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "each_key", env_each_key.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "each_value", env_each_value.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "delete", env_delete_m.singleton, 1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "delete_if", env_delete_if.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "clear", env_clear.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "reject", env_reject.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "reject!", env_reject_bang.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "select", env_select.singleton, -1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "shift", env_shift.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "invert", env_invert.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "replace", env_replace.singleton, 1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "update", env_update.singleton, 1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "inspect", env_inspect.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "rehash", env_none.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "to_a", env_to_a.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "to_s", env_to_s.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "index", env_index.singleton, 1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "indexes", env_indexes.singleton, -1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "indices", env_indexes.singleton, -1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "size", env_size.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "length", env_size.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "empty?", env_empty_p.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "keys", env_keys.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "values", env_values.singleton, 0, null);
            Class.rb_define_singleton_method(Hash.envtbl, "values_at", env_values_at.singleton, -1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "include?", env_has_key.singleton, 1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "member?", env_has_key.singleton, 1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "has_key?", env_has_key.singleton, 1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "has_value?", env_has_value.singleton, 1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "key?", env_has_key.singleton, 1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "value?", env_has_value.singleton, 1, null);
            Class.rb_define_singleton_method(Hash.envtbl, "to_hash", env_to_hash.singleton, 0, null);

            Variables.rb_define_global_const("ENV", Hash.envtbl);
        }

        static void Init_Struct()
        {
            rb_cStruct = Class.rb_define_class("Struct", rb_cObject, null);
            Class.rb_include_module(null, rb_cStruct, rb_mEnumerable);

            Class.rb_undef_alloc_func(rb_cStruct);
            Class.rb_define_singleton_method(rb_cStruct, "new", rb_struct_s_def.singleton, 1, null);

            Class.rb_define_method(rb_cStruct, "initialize", rb_struct_initialize.singleton, 2, null);
            Class.rb_define_method(rb_cStruct, "initialize_copy", rb_struct_init_copy.singleton, 1, null);

            Class.rb_define_method(rb_cStruct, "==", rb_struct_equal.singleton, 1, null);
            Class.rb_define_method(rb_cStruct, "eql?", rb_struct_eql.singleton, 1, null);
            Class.rb_define_method(rb_cStruct, "hash", rb_struct_hash.singleton, 0, null);

            Class.rb_define_method(rb_cStruct, "to_s", rb_struct_inspect.singleton, 0, null);
            Class.rb_define_method(rb_cStruct, "inspect", rb_struct_inspect.singleton, 0, null);
            Class.rb_define_method(rb_cStruct, "to_a", rb_struct_to_a.singleton, 0, null);
            Class.rb_define_method(rb_cStruct, "values", rb_struct_to_a.singleton, 0, null);
            Class.rb_define_method(rb_cStruct, "size", rb_struct_size.singleton, 0, null);
            Class.rb_define_method(rb_cStruct, "length", rb_struct_size.singleton, 0, null);

            Class.rb_define_method(rb_cStruct, "each", rb_struct_each.singleton, 0, null);
            Class.rb_define_method(rb_cStruct, "each_pair", rb_struct_each_pair.singleton, 0, null);
            Class.rb_define_method(rb_cStruct, "[]", rb_struct_aref.singleton, 1, null);
            Class.rb_define_method(rb_cStruct, "[]=", rb_struct_aset.singleton, 2, null);
            Class.rb_define_method(rb_cStruct, "select", rb_struct_select.singleton, 1, null);
            Class.rb_define_method(rb_cStruct, "values_at", rb_struct_values_at.singleton, 1, null);

            Class.rb_define_method(rb_cStruct, "members", rb_struct_members_m.singleton, 0, null);
        }

        static void Init_Regexp()
        {
            rb_eRegexpError = Class.rb_define_class("RegexpError", rb_eStandardError, null);

            //re_set_casetable(casetable);

            Regexp.re_mbcinit(Regexp.MBCTYPE_ASCII);

            Variables.rb_define_variable("$~", Regexp.match_glob); // backref
            Variables.rb_define_variable("$&", Regexp.last_match_glob);
            Variables.rb_define_variable("$`", Regexp.prematch);
            Variables.rb_define_variable("$'", Regexp.postmatch);
            Variables.rb_define_variable("$+", Regexp.last_paren_match);

            Variables.rb_define_variable("$=", Regexp.ignorecase);
            Variables.rb_define_variable("$KCODE", Regexp.kcode_glob);
            Variables.rb_define_variable("$-K", Regexp.kcode_glob);

            rb_cRegexp = Class.rb_define_class("Regexp", rb_cObject, null);
            Class.rb_define_alloc_func(rb_cRegexp, rb_reg_s_alloc.singleton);
            Class.rb_define_singleton_method(rb_cRegexp, "compile", rb_class_new_instance.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cRegexp, "quote", rb_reg_s_quote.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cRegexp, "escape", rb_reg_s_quote.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cRegexp, "union", rb_reg_s_union.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cRegexp, "last_match", rb_reg_s_last_match.singleton, 1, null);

            Class.rb_define_method(rb_cRegexp, "initialize", rb_reg_initialize_m.singleton, 1, null);
            Class.rb_define_method(rb_cRegexp, "initialize_copy", rb_reg_init_copy.singleton, 1, null);
            Class.rb_define_method(rb_cRegexp, "hash", rb_reg_hash.singleton, 0, null);
            Class.rb_define_method(rb_cRegexp, "eql?", rb_reg_equal.singleton, 1, null);
            Class.rb_define_method(rb_cRegexp, "==", rb_reg_equal.singleton, 1, null);
            Class.rb_define_method(rb_cRegexp, "=~", rb_reg_match.singleton, 1, null);
            Class.rb_define_method(rb_cRegexp, "===", rb_reg_eqq.singleton, 1, null);
            Class.rb_define_method(rb_cRegexp, "~", rb_reg_match2.singleton, 0, null);
            Class.rb_define_method(rb_cRegexp, "match", rb_reg_match_m.singleton, 1, null);
            Class.rb_define_method(rb_cRegexp, "to_s", rb_reg_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cRegexp, "inspect", rb_reg_inspect.singleton, 0, null);
            Class.rb_define_method(rb_cRegexp, "source", rb_reg_source.singleton, 0, null);
            Class.rb_define_method(rb_cRegexp, "casefold?", rb_reg_casefold_p.singleton, 0, null);
            Class.rb_define_method(rb_cRegexp, "options", rb_reg_options_m.singleton, 0, null);
            Class.rb_define_method(rb_cRegexp, "kcode", rb_reg_kcode_m.singleton, 0, null);

            Variables.rb_define_const(rb_cRegexp, "IGNORECASE", (Ruby.Regexp.RE_OPTION.IGNORECASE));
            Variables.rb_define_const(rb_cRegexp, "EXTENDED", (Ruby.Regexp.RE_OPTION.EXTENDED));
            Variables.rb_define_const(rb_cRegexp, "MULTILINE", (Ruby.Regexp.RE_OPTION.MULTILINE));

            //rb_global_variable(RE.reg_cache);

            rb_cMatch = Class.rb_define_class("MatchData", rb_cObject, null);
            Variables.rb_define_global_const("MatchingData", rb_cMatch);
            Class.rb_define_alloc_func(rb_cMatch, match_alloc.singleton);
            Class.rb_undef_method(Class.CLASS_OF(rb_cMatch), "new");

            Class.rb_define_method(rb_cMatch, "initialize_copy", match_init_copy.singleton, 1, null);
            Class.rb_define_method(rb_cMatch, "size", match_size.singleton, 0, null);
            Class.rb_define_method(rb_cMatch, "length", match_size.singleton, 0, null);
            Class.rb_define_method(rb_cMatch, "offset", match_offset.singleton, 1, null);
            Class.rb_define_method(rb_cMatch, "begin", match_begin.singleton, 1, null);
            Class.rb_define_method(rb_cMatch, "end", match_end.singleton, 1, null);
            Class.rb_define_method(rb_cMatch, "to_a", match_to_a.singleton, 0, null);
            Class.rb_define_method(rb_cMatch, "[]", match_aref.singleton, 1, null);
            Class.rb_define_method(rb_cMatch, "captures", match_captures.singleton, 0, null);
            Class.rb_define_method(rb_cMatch, "select", match_select.singleton, 1, null);
            Class.rb_define_method(rb_cMatch, "values_at", match_values_at.singleton, 1, null);
            Class.rb_define_method(rb_cMatch, "pre_match", rb_reg_match_pre.singleton, 0, null);
            Class.rb_define_method(rb_cMatch, "post_match", rb_reg_match_post.singleton, 0, null);
            Class.rb_define_method(rb_cMatch, "to_s", match_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cMatch, "inspect", rb_any_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cMatch, "string", match_string.singleton, 0, null);
        }

        static void Init_pack()
        {
            Class.rb_define_method(rb_cArray, "pack", pack_pack.singleton, 1, null);
            Class.rb_define_method(rb_cString, "unpack", pack_unpack.singleton, 1, null);
        }

        static void Init_Range()
        {
            rb_cRange = Class.rb_define_class("Range", rb_cObject, null);
            Class.rb_include_module(null, rb_cRange, rb_mEnumerable);
            Class.rb_define_alloc_func(rb_cRange, range_alloc.singleton);
            Class.rb_define_method(rb_cRange, "initialize", range_initialize.singleton, 1, null);
            Class.rb_define_method(rb_cRange, "==", range_eq.singleton, 1, null);
            Class.rb_define_method(rb_cRange, "===", range_include.singleton, 1, null);
            Class.rb_define_method(rb_cRange, "eql?", range_eql.singleton, 1, null);
            Class.rb_define_method(rb_cRange, "hash", range_hash.singleton, 0, null);
            Class.rb_define_method(rb_cRange, "each", range_each.singleton, 0, null);
            Class.rb_define_method(rb_cRange, "step", range_step.singleton, -1, null);
            Class.rb_define_method(rb_cRange, "first", range_first.singleton, 0, null);
            Class.rb_define_method(rb_cRange, "last", range_last.singleton, 0, null);
            Class.rb_define_method(rb_cRange, "begin", range_first.singleton, 0, null);
            Class.rb_define_method(rb_cRange, "end", range_last.singleton, 0, null);
            Class.rb_define_method(rb_cRange, "to_s", range_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cRange, "inspect", range_inspect.singleton, 0, null);

            Class.rb_define_method(rb_cRange, "exclude_end?", range_exclude_end_p.singleton, 0, null);

            Class.rb_define_method(rb_cRange, "member?", range_include.singleton, 1, null);
            Class.rb_define_method(rb_cRange, "include?", range_include.singleton, 1, null);

            //id_cmp = rb_intern("<=>");
            //id_succ = rb_intern("succ");
            //id_beg = rb_intern("begin");
            //id_end = rb_intern("end");
            //id_excl = rb_intern("excl");
        }

        static void Init_IO()
        {
            rb_eIOError = Class.rb_define_class("IOError", rb_eStandardError, null);
            rb_eEOFError = Class.rb_define_class("EOFError", rb_eIOError, null);

            //id_write = rb_intern("write");
            //id_read = rb_intern("read");
            //id_getc = rb_intern("getc");

            Class.rb_define_global_function("syscall", rb_f_syscall.singleton, -1, null);

            Class.rb_define_global_function("open", rb_f_open.singleton, -1, null);
            Class.rb_define_global_function("printf", rb_f_printf.singleton, -1, null);
            Class.rb_define_global_function("print", rb_f_print.singleton, -1, null);
            Class.rb_define_global_function("putc", rb_f_putc.singleton, 1, null);
            Class.rb_define_global_function("puts", rb_f_puts.singleton, -1, null);
            Class.rb_define_global_function("gets", rb_f_gets.singleton, -1, null);
            Class.rb_define_global_function("readline", rb_f_readline.singleton, -1, null);
            Class.rb_define_global_function("getc", rb_f_getc.singleton, 0, null);
            Class.rb_define_global_function("select", rb_f_select.singleton, -1, null);

            Class.rb_define_global_function("readlines", rb_f_readlines.singleton, -1, null);

            Class.rb_define_global_function("`", rb_f_backquote.singleton, 1, null);

            Class.rb_define_global_function("p", rb_f_p.singleton, -1, null);
            Class.rb_define_method(rb_mKernel, "display", rb_obj_display.singleton, -1, null);

            rb_cIO = Class.rb_define_class("IO", rb_cObject, null);
            Class.rb_include_module(null, rb_cIO, rb_mEnumerable);

            Class.rb_define_alloc_func(rb_cIO, io_alloc.singleton);
            Class.rb_define_singleton_method(rb_cIO, "new", rb_io_s_new.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cIO, "open", rb_io_s_open.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cIO, "sysopen", rb_io_s_sysopen.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cIO, "for_fd", rb_io_s_for_fd.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cIO, "popen", rb_io_s_popen.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cIO, "foreach", rb_io_s_foreach.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cIO, "readlines", rb_io_s_readlines.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cIO, "read", rb_io_s_read.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cIO, "select", rb_f_select.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cIO, "pipe", rb_io_s_pipe.singleton, 0, null);

            Class.rb_define_method(rb_cIO, "initialize", rb_io_initialize.singleton, 1, null);

            IO.rb_output_fs.value = null;
            Variables.rb_define_variable("$,", IO.rb_output_fs);

            IO.rb_rs.value = IO.rb_default_rs = new String("\n");
            IO.rb_output_rs.value = null;
            //rb_global_variable(IO.rb_default_rs);
            Object.rb_obj_freeze(null, IO.rb_default_rs);
            Variables.rb_define_variable("$/", IO.rb_rs);
            Variables.rb_define_variable("$-0", IO.rb_rs);
            Variables.rb_define_variable("$\\", IO.rb_output_rs);

            Variables.rb_define_variable("$.", IO.lineno_global);
            Variables.rb_define_variable("$_", Eval.rb_lastline);

            Class.rb_define_method(rb_cIO, "initialize_copy", rb_io_init_copy.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "reopen", rb_io_reopen.singleton, 1, null);

            Class.rb_define_method(rb_cIO, "print", rb_io_print.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "putc", rb_io_putc.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "puts", rb_io_puts.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "printf", rb_io_printf.singleton, 1, null);

            Class.rb_define_method(rb_cIO, "each", rb_io_each_line.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "each_line", rb_io_each_line.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "each_byte", rb_io_each_byte.singleton, 0, null);

            Class.rb_define_method(rb_cIO, "syswrite", rb_io_syswrite.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "sysread", rb_io_sysread.singleton, 1, null);

            Class.rb_define_method(rb_cIO, "fileno", rb_io_fileno.singleton, 0, null);
            Class.rb_define_alias(rb_cIO, "to_i", "fileno", null);
            Class.rb_define_method(rb_cIO, "to_io", rb_io_to_io.singleton, 0, null);

            Class.rb_define_method(rb_cIO, "fsync", rb_io_fsync.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "sync", rb_io_sync.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "sync=", rb_io_set_sync.singleton, 1, null);

            Class.rb_define_method(rb_cIO, "lineno", rb_io_lineno.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "lineno=", rb_io_set_lineno.singleton, 1, null);

            Class.rb_define_method(rb_cIO, "readlines", rb_io_readlines.singleton, 1, null);

            Class.rb_define_method(rb_cIO, "read", io_read.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "write", io_write.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "gets", rb_io_gets_m.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "readline", rb_io_readline.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "getc", rb_io_getc.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "readchar", rb_io_readchar.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "ungetc", rb_io_ungetc.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "<<", rb_io_addstr.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "flush", rb_io_flush.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "tell", rb_io_tell.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "seek", rb_io_seek_m.singleton, 1, null);
            Variables.rb_define_const(rb_cIO, "SEEK_SET", (IO.SEEK_SET));
            Variables.rb_define_const(rb_cIO, "SEEK_CUR", (IO.SEEK_CUR));
            Variables.rb_define_const(rb_cIO, "SEEK_END", (IO.SEEK_END));
            Class.rb_define_method(rb_cIO, "rewind", rb_io_rewind.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "pos", rb_io_tell.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "pos=", rb_io_set_pos.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "eof", rb_io_eof.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "eof?", rb_io_eof.singleton, 0, null);

            Class.rb_define_method(rb_cIO, "close", rb_io_close_m.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "closed?", rb_io_closed.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "close_read", rb_io_close_read.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "close_write", rb_io_close_write.singleton, 0, null);

            Class.rb_define_method(rb_cIO, "isatty", rb_io_isatty.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "tty?", rb_io_isatty.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "binmode", rb_io_binmode.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "sysseek", rb_io_sysseek.singleton, 1, null);

            Class.rb_define_method(rb_cIO, "ioctl", rb_io_ioctl.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "fcntl", rb_io_fcntl.singleton, 1, null);
            Class.rb_define_method(rb_cIO, "pid", rb_io_pid.singleton, 0, null);
            Class.rb_define_method(rb_cIO, "inspect", rb_io_inspect.singleton, 0, null);

            IO.rb_stdin.value = IO.prep_stdio(System.Console.OpenStandardInput(), IO.FMODE_READABLE, rb_cIO);
            Variables.rb_define_variable("$stdin", IO.rb_stdin);
            IO.rb_stdout.value = IO.prep_stdio(System.Console.OpenStandardOutput(), IO.FMODE_WRITABLE, rb_cIO);
            Variables.rb_define_variable("$stdout", IO.rb_stdout);
            IO.rb_stderr.value = IO.prep_stdio(System.Console.OpenStandardError(), IO.FMODE_WRITABLE, rb_cIO);
            Variables.rb_define_variable("$stderr", IO.rb_stderr);
            Variables.rb_define_variable("$>", IO.rb_stdout);
            IO.orig_stdout = (IO)IO.rb_stdout.value;
            IO.orig_stderr = (IO)IO.rb_stderr.value;

            Variables.rb_define_variable("$defout", IO.defout);
            Variables.rb_define_variable("$deferr", IO.deferr);

            Variables.rb_define_global_const("STDIN", IO.rb_stdin.value);
            Variables.rb_define_global_const("STDOUT", IO.rb_stdout.value);
            Variables.rb_define_global_const("STDERR", IO.rb_stderr.value);

            // IO.argf.value = new Object();
            // Class.rb_extend_object(IO.argf, rb_mEnumerable);

            IO.argf.value = new Object();
            Class.rb_extend_object(null, IO.argf.value, rb_mEnumerable);

            Variables.rb_define_variable("$<", IO.argf);
            Variables.rb_define_global_const("ARGF", IO.argf.value);

            Class.rb_define_singleton_method(IO.argf.value, "to_s", argf_to_s.singleton, 0, null);

            Class.rb_define_singleton_method(IO.argf.value, "fileno", argf_fileno.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "to_i", argf_fileno.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "to_io", argf_to_io.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "each", argf_each_line.singleton, -1, null);
            Class.rb_define_singleton_method(IO.argf.value, "each_line", argf_each_line.singleton, -1, null);
            Class.rb_define_singleton_method(IO.argf.value, "each_byte", argf_each_byte.singleton, 0, null);

            Class.rb_define_singleton_method(IO.argf.value, "read", argf_read.singleton, -1, null);
            Class.rb_define_singleton_method(IO.argf.value, "readlines", rb_f_readlines.singleton, -1, null);
            Class.rb_define_singleton_method(IO.argf.value, "to_a", rb_f_readlines.singleton, -1, null);
            Class.rb_define_singleton_method(IO.argf.value, "gets", rb_f_gets.singleton, -1, null);
            Class.rb_define_singleton_method(IO.argf.value, "readline", rb_f_readline.singleton, -1, null);
            Class.rb_define_singleton_method(IO.argf.value, "getc", argf_getc.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "readchar", argf_readchar.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "tell", argf_tell.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "seek", argf_seek_m.singleton, -1, null);
            Class.rb_define_singleton_method(IO.argf.value, "rewind", argf_rewind.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "pos", argf_tell.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "pos=", argf_set_pos.singleton, 1, null);
            Class.rb_define_singleton_method(IO.argf.value, "eof", argf_eof.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "eof?", argf_eof.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "binmode", argf_binmode.singleton, 0, null);

            Class.rb_define_singleton_method(IO.argf.value, "filename", argf_filename.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "path", argf_filename.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "file", argf_file.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "skip", argf_skip.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "close", argf_close_m.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "closed?", argf_closed.singleton, 0, null);

            Class.rb_define_singleton_method(IO.argf.value, "lineno", argf_lineno.singleton, 0, null);
            Class.rb_define_singleton_method(IO.argf.value, "lineno=", argf_set_lineno.singleton, 1, null);

            //rb_global_variable( IO.current_file);
            IO.filename.value = new String("-");
            Variables.rb_define_variable("$FILENAME", IO.filename);

            Variables.rb_define_variable("$-i", IO.opt_i);

            //Win32.atexit(IO.pipe_atexit);

            Init_File();

            Class.rb_define_method(rb_cFile, "initialize", rb_file_initialize.singleton, 1, null);

            File.rb_file_const("RDONLY", (File.O_RDONLY));
            File.rb_file_const("WRONLY", (File.O_WRONLY));
            File.rb_file_const("RDWR", (File.O_RDWR));
            File.rb_file_const("APPEND", (File.O_APPEND));
            File.rb_file_const("CREAT", (File.O_CREAT));
            File.rb_file_const("EXCL", (File.O_EXCL));
            File.rb_file_const("TRUNC", (File.O_TRUNC));
            File.rb_file_const("BINARY", (File.O_BINARY));
        }

        static void Init_File()
        {
            rb_mFileTest = Class.rb_define_module("FileTest", null);
            rb_cFile = Class.rb_define_class("File", rb_cIO, null);

            File.define_filetest_function("directory?", test_d.singleton, 1);
            File.define_filetest_function("exist?", test_e.singleton, 1);
            File.define_filetest_function("exists?", test_e.singleton, 1);
            File.define_filetest_function("readable?", test_r.singleton, 1);
            File.define_filetest_function("readable_real?", test_R.singleton, 1);
            File.define_filetest_function("writable?", test_w.singleton, 1);
            File.define_filetest_function("writable_real?", test_W.singleton, 1);
            File.define_filetest_function("executable?", test_x.singleton, 1);
            File.define_filetest_function("executable_real?", test_X.singleton, 1);
            File.define_filetest_function("file?", test_f.singleton, 1);
            File.define_filetest_function("zero?", test_z.singleton, 1);
            File.define_filetest_function("size?", test_s.singleton, 1);
            File.define_filetest_function("size", rb_file_s_size.singleton, 1);
            File.define_filetest_function("owned?", test_owned.singleton, 1);
            File.define_filetest_function("grpowned?", test_grpowned.singleton, 1);

            File.define_filetest_function("pipe?", test_p.singleton, 1);
            File.define_filetest_function("symlink?", test_l.singleton, 1);
            File.define_filetest_function("socket?", test_S.singleton, 1);

            File.define_filetest_function("blockdev?", test_b.singleton, 1);
            File.define_filetest_function("chardev?", test_c.singleton, 1);

            File.define_filetest_function("setuid?", test_suid.singleton, 1);
            File.define_filetest_function("setgid?", test_sgid.singleton, 1);
            File.define_filetest_function("sticky?", test_sticky.singleton, 1);

            Class.rb_define_singleton_method(rb_cFile, "stat", rb_file_s_stat.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "lstat", rb_file_s_lstat.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "ftype", rb_file_s_ftype.singleton, 1, null);

            Class.rb_define_singleton_method(rb_cFile, "atime", rb_file_s_atime.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "mtime", rb_file_s_mtime.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "ctime", rb_file_s_ctime.singleton, 1, null);

            Class.rb_define_singleton_method(rb_cFile, "utime", rb_file_s_utime.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "chmod", rb_file_s_chmod.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "chown", rb_file_s_chown.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "lchmod", rb_file_s_lchmod.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "lchown", rb_file_s_lchown.singleton, 1, null);

            Class.rb_define_singleton_method(rb_cFile, "link", rb_file_s_link.singleton, 2, null);
            Class.rb_define_singleton_method(rb_cFile, "symlink", rb_file_s_symlink.singleton, 2, null);
            Class.rb_define_singleton_method(rb_cFile, "readlink", rb_file_s_readlink.singleton, 1, null);

            Class.rb_define_singleton_method(rb_cFile, "unlink", rb_file_s_unlink.singleton, 2, null);
            Class.rb_define_singleton_method(rb_cFile, "delete", rb_file_s_unlink.singleton, 2, null);
            Class.rb_define_singleton_method(rb_cFile, "rename", rb_file_s_rename.singleton, 2, null);
            Class.rb_define_singleton_method(rb_cFile, "umask", rb_file_s_umask.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "truncate", rb_file_s_truncate.singleton, 2, null);
            Class.rb_define_singleton_method(rb_cFile, "expand_path", rb_file_s_expand_path.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "basename", rb_file_s_basename.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "dirname", rb_file_s_dirname.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "extname", rb_file_s_extname.singleton, 1, null);

            File.separator = Object.rb_obj_freeze(null, new String("/"));
            Variables.rb_define_const(rb_cFile, "Separator", File.separator);
            Variables.rb_define_const(rb_cFile, "SEPARATOR", File.separator);
            Class.rb_define_singleton_method(rb_cFile, "split", rb_file_s_split.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "join", rb_file_s_join.singleton, 2, null);

            Variables.rb_define_const(rb_cFile, "ALT_SEPARATOR", Object.rb_obj_freeze(null, new String("\\")));

            Variables.rb_define_const(rb_cFile, "PATH_SEPARATOR", Object.rb_obj_freeze(null, new String(File.PATH_SEP)));

            Class.rb_define_method(rb_cIO, "stat", rb_io_stat.singleton, 0, null);
            Class.rb_define_method(rb_cFile, "lstat", rb_file_lstat.singleton, 0, null);

            Class.rb_define_method(rb_cFile, "atime", rb_file_atime.singleton, 0, null);
            Class.rb_define_method(rb_cFile, "mtime", rb_file_mtime.singleton, 0, null);
            Class.rb_define_method(rb_cFile, "ctime", rb_file_ctime.singleton, 0, null);

            Class.rb_define_method(rb_cFile, "chmod", rb_file_chmod.singleton, 1, null);
            Class.rb_define_method(rb_cFile, "chown", rb_file_chown.singleton, 2, null);
            Class.rb_define_method(rb_cFile, "truncate", rb_file_truncate.singleton, 1, null);

            Class.rb_define_method(rb_cFile, "flock", rb_file_flock.singleton, 1, null);

            rb_mFConst = Class.rb_define_module_under(rb_cFile, "Constants", null);
            Class.rb_include_module(null, rb_cIO, rb_mFConst);
            File.rb_file_const("LOCK_SH", (File.LOCK_SH));
            File.rb_file_const("LOCK_EX", (File.LOCK_EX));
            File.rb_file_const("LOCK_UN", (File.LOCK_UN));
            File.rb_file_const("LOCK_NB", (File.LOCK_NB));

            Class.rb_define_method(rb_cFile, "path", rb_file_path.singleton, 0, null);
            Class.rb_define_global_function("test", rb_f_test.singleton, -1, null);

            rb_cStat = Class.rb_define_class_under(rb_cFile, "Stat", rb_cObject, null);
            Class.rb_define_alloc_func(rb_cStat, rb_stat_s_alloc.singleton);
            Class.rb_define_method(rb_cStat, "initialize", rb_stat_init.singleton, 1, null);
            Class.rb_define_method(rb_cStat, "initialize_copy", rb_stat_init_copy.singleton, 1, null);

            Class.rb_include_module(null, rb_cStat, rb_mComparable);

            Class.rb_define_method(rb_cStat, "<=>", rb_stat_cmp.singleton, 1, null);

            Class.rb_define_method(rb_cStat, "dev", rb_stat_dev.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "dev_major", rb_stat_dev_major.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "dev_minor", rb_stat_dev_minor.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "ino", rb_stat_ino.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "mode", rb_stat_mode.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "nlink", rb_stat_nlink.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "uid", rb_stat_uid.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "gid", rb_stat_gid.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "rdev", rb_stat_rdev.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "rdev_major", rb_stat_rdev_major.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "rdev_minor", rb_stat_rdev_minor.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "size", rb_stat_size.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "blksize", rb_stat_blksize.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "blocks", rb_stat_blocks.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "atime", rb_stat_atime.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "mtime", rb_stat_mtime.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "ctime", rb_stat_ctime.singleton, 0, null);

            Class.rb_define_method(rb_cStat, "inspect", rb_stat_inspect.singleton, 0, null);

            Class.rb_define_method(rb_cStat, "ftype", rb_stat_ftype.singleton, 0, null);

            Class.rb_define_method(rb_cStat, "directory?", rb_stat_d.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "readable?", rb_stat_r.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "readable_real?", rb_stat_R.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "writable?", rb_stat_w.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "writable_real?", rb_stat_W.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "executable?", rb_stat_x.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "executable_real?", rb_stat_X.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "file?", rb_stat_f.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "zero?", rb_stat_z.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "size?", rb_stat_s.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "owned?", rb_stat_owned.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "grpowned?", rb_stat_grpowned.singleton, 0, null);

            Class.rb_define_method(rb_cStat, "pipe?", rb_stat_p.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "symlink?", rb_stat_l.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "socket?", rb_stat_S.singleton, 0, null);

            Class.rb_define_method(rb_cStat, "blockdev?", rb_stat_b.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "chardev?", rb_stat_c.singleton, 0, null);

            Class.rb_define_method(rb_cStat, "setuid?", rb_stat_suid.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "setgid?", rb_stat_sgid.singleton, 0, null);
            Class.rb_define_method(rb_cStat, "sticky?", rb_stat_sticky.singleton, 0, null);
        }

        static void Init_Dir()
        {
            rb_cDir = Class.rb_define_class("Dir", rb_cObject, null);

            Class.rb_include_module(null, rb_cDir, rb_mEnumerable);

            Class.rb_define_alloc_func(rb_cDir, dir_s_alloc.singleton);
            Class.rb_define_singleton_method(rb_cDir, "open", dir_s_open.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cDir, "foreach", dir_foreach.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cDir, "entries", dir_entries.singleton, 1, null);

            Class.rb_define_method(rb_cDir, "initialize", dir_initialize.singleton, 1, null);
            Class.rb_define_method(rb_cDir, "path", dir_path.singleton, 0, null);
            Class.rb_define_method(rb_cDir, "read", dir_read.singleton, 0, null);
            Class.rb_define_method(rb_cDir, "each", dir_each.singleton, 0, null);
            Class.rb_define_method(rb_cDir, "rewind", dir_rewind.singleton, 0, null);
            Class.rb_define_method(rb_cDir, "tell", dir_tell.singleton, 0, null);
            Class.rb_define_method(rb_cDir, "seek", dir_seek.singleton, 1, null);
            Class.rb_define_method(rb_cDir, "pos", dir_tell.singleton, 0, null);
            Class.rb_define_method(rb_cDir, "pos=", dir_set_pos.singleton, 1, null);
            Class.rb_define_method(rb_cDir, "close", dir_close.singleton, 0, null);

            Class.rb_define_singleton_method(rb_cDir, "chdir", dir_s_chdir.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cDir, "getwd", dir_s_getwd.singleton, 0, null);
            Class.rb_define_singleton_method(rb_cDir, "pwd", dir_s_getwd.singleton, 0, null);
            Class.rb_define_singleton_method(rb_cDir, "chroot", dir_s_chroot.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cDir, "mkdir", dir_s_mkdir.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cDir, "rmdir", dir_s_rmdir.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cDir, "delete", dir_s_rmdir.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cDir, "unlink", dir_s_rmdir.singleton, 1, null);

            Class.rb_define_singleton_method(rb_cDir, "glob", dir_s_glob.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cDir, "[]", dir_s_aref.singleton, 1, null);

            Class.rb_define_singleton_method(rb_cFile, "fnmatch", file_s_fnmatch.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cFile, "fnmatch?", file_s_fnmatch.singleton, 1, null);

            File.rb_file_const("FNM_NOESCAPE", (Dir.FNM_NOESCAPE));
            File.rb_file_const("FNM_PATHNAME", (Dir.FNM_PATHNAME));
            File.rb_file_const("FNM_DOTMATCH", (Dir.FNM_DOTMATCH));
            File.rb_file_const("FNM_CASEFOLD", (Dir.FNM_CASEFOLD));
        }

        static void Init_Time()
        {
            rb_cTime = Class.rb_define_class("Time", rb_cObject, null);
            Class.rb_include_module(null, rb_cTime, rb_mComparable);

            Class.rb_define_alloc_func(rb_cTime, time_s_alloc.singleton);
            Class.rb_define_singleton_method(rb_cTime, "now", rb_class_new_instance.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cTime, "at", time_s_at.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cTime, "utc", time_s_mkutc.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cTime, "gm", time_s_mkutc.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cTime, "local", time_s_mktime.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cTime, "mktime", time_s_mktime.singleton, 1, null);

            Class.rb_define_singleton_method(rb_cTime, "times", time_s_times.singleton, 0, null);

            Class.rb_define_method(rb_cTime, "to_i", time_to_i.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "to_f", time_to_f.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "<=>", time_cmp.singleton, 1, null);
            Class.rb_define_method(rb_cTime, "eql?", time_eql.singleton, 1, null);
            Class.rb_define_method(rb_cTime, "hash", time_hash.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "initialize", time_init.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "initialize_copy", time_init_copy.singleton, 1, null);

            Class.rb_define_method(rb_cTime, "localtime", time_localtime.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "gmtime", time_gmtime.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "utc", time_gmtime.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "getlocal", time_getlocaltime.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "getgm", time_getgmtime.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "getutc", time_getgmtime.singleton, 0, null);

            Class.rb_define_method(rb_cTime, "ctime", time_asctime.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "asctime", time_asctime.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "to_s", time_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "inspect", time_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "to_a", time_to_a.singleton, 0, null);

            Class.rb_define_method(rb_cTime, "+", time_plus.singleton, 1, null);
            Class.rb_define_method(rb_cTime, "-", time_minus.singleton, 1, null);

            Class.rb_define_method(rb_cTime, "succ", time_succ.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "sec", time_sec.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "min", time_min.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "hour", time_hour.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "mday", time_mday.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "day", time_mday.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "mon", time_mon.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "month", time_mon.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "year", time_year.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "wday", time_wday.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "yday", time_yday.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "isdst", time_isdst.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "dst?", time_isdst.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "zone", time_zone.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "gmtoff", time_utc_offset.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "gmt_offset", time_utc_offset.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "utc_offset", time_utc_offset.singleton, 0, null);

            Class.rb_define_method(rb_cTime, "utc?", time_utc_p.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "gmt?", time_utc_p.singleton, 0, null);

            Class.rb_define_method(rb_cTime, "tv_sec", time_to_i.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "tv_usec", time_usec.singleton, 0, null);
            Class.rb_define_method(rb_cTime, "usec", time_usec.singleton, 0, null);

            Class.rb_define_method(rb_cTime, "strftime", time_strftime.singleton, 1, null);

            Class.rb_define_method(rb_cTime, "_dump", time_dump.singleton, 1, null);
            Class.rb_define_singleton_method(rb_cTime, "_load", time_load.singleton, 1, null);
        }

        static void Init_Random()
        {
            Class.rb_define_global_function("srand", rb_f_srand.singleton, -1, null);
            Class.rb_define_global_function("rand", rb_f_rand.singleton, -1, null);
        }

        static void Init_signal()
        {
            rb_mSignal = Class.rb_define_module("Signal", null);

            Class.rb_define_global_function("trap", sig_trap.singleton, -1, null);
            Class.rb_define_module_function(rb_mSignal, "trap", sig_trap.singleton, 1, null);
            Class.rb_define_module_function(rb_mSignal, "list", sig_list.singleton, 0, null);

            Signal.install_sighandler(Signal.SIGINT, Signal.sighandler);
            //Signal.install_sighandler(Signal.SIGSEGV, Signal.sigsegv);
        }

        static void Init_process()
        {
            Variables.rb_define_variable("$$", Process.get_pid);
            Variables.rb_define_variable("$?", Process.rb_last_status);
            Class.rb_define_global_function("exec", rb_f_exec.singleton, -1, null);
            Class.rb_define_global_function("fork", rb_f_fork.singleton, 0, null);
            Class.rb_define_global_function("exit!", rb_f_exit_bang.singleton, -1, null);
            Class.rb_define_global_function("system", rb_f_system.singleton, -1, null);
            Class.rb_define_global_function("sleep", rb_f_sleep.singleton, -1, null);

            rb_mProcess = Class.rb_define_module("Process", null);

            Class.rb_define_singleton_method(rb_mProcess, "fork", rb_f_fork.singleton, 0, null);
            Class.rb_define_singleton_method(rb_mProcess, "exit!", rb_f_exit_bang.singleton, -1, null);
            Class.rb_define_singleton_method(rb_mProcess, "exit", rb_f_exit.singleton, -1, null);
            Class.rb_define_singleton_method(rb_mProcess, "abort", rb_f_abort.singleton, -1, null);

            Class.rb_define_module_function(rb_mProcess, "kill", rb_f_kill.singleton, -1, null);
            Class.rb_define_module_function(rb_mProcess, "wait", proc_wait.singleton, -1, null);
            Class.rb_define_module_function(rb_mProcess, "wait2", proc_wait2.singleton, -1, null);
            Class.rb_define_module_function(rb_mProcess, "waitpid", proc_wait.singleton, -1, null);
            Class.rb_define_module_function(rb_mProcess, "waitpid2", proc_wait2.singleton, -1, null);
            Class.rb_define_module_function(rb_mProcess, "waitall", proc_waitall.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcess, "detach", proc_detach.singleton, 1, null);

            rb_cProcStatus = Class.rb_define_class_under(rb_mProcess, "Status", rb_cObject, null);
            Class.rb_undef_method(Class.CLASS_OF(rb_cProcStatus), "new");

            Class.rb_define_method(rb_cProcStatus, "==", pst_equal.singleton, 1, null);
            Class.rb_define_method(rb_cProcStatus, "&", pst_bitand.singleton, 1, null);
            Class.rb_define_method(rb_cProcStatus, ">>", pst_rshift.singleton, 1, null);
            Class.rb_define_method(rb_cProcStatus, "to_i", pst_to_i.singleton, 0, null);
            Class.rb_define_method(rb_cProcStatus, "to_int", pst_to_i.singleton, 0, null);
            Class.rb_define_method(rb_cProcStatus, "to_s", pst_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cProcStatus, "inspect", pst_inspect.singleton, 0, null);

            Class.rb_define_method(rb_cProcStatus, "pid", pst_pid.singleton, 0, null);

            Class.rb_define_method(rb_cProcStatus, "stopped?", pst_wifstopped.singleton, 0, null);
            Class.rb_define_method(rb_cProcStatus, "stopsig", pst_wstopsig.singleton, 0, null);
            Class.rb_define_method(rb_cProcStatus, "signaled?", pst_wifsignaled.singleton, 0, null);
            Class.rb_define_method(rb_cProcStatus, "termsig", pst_wtermsig.singleton, 0, null);
            Class.rb_define_method(rb_cProcStatus, "exited?", pst_wifexited.singleton, 0, null);
            Class.rb_define_method(rb_cProcStatus, "exitstatus", pst_wexitstatus.singleton, 0, null);
            Class.rb_define_method(rb_cProcStatus, "success?", pst_success_p.singleton, 0, null);
            Class.rb_define_method(rb_cProcStatus, "coredump?", pst_wcoredump.singleton, 0, null);

            Class.rb_define_module_function(rb_mProcess, "pid", get_pid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcess, "ppid", get_ppid.singleton, 0, null);

            Class.rb_define_module_function(rb_mProcess, "getpgrp", proc_getpgrp.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcess, "setpgrp", proc_setpgrp.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcess, "getpgid", proc_getpgid.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcess, "setpgid", proc_setpgid.singleton, 2, null);

            Class.rb_define_module_function(rb_mProcess, "setsid", proc_setsid.singleton, 0, null);

            Class.rb_define_module_function(rb_mProcess, "getpriority", proc_getpriority.singleton, 2, null);
            Class.rb_define_module_function(rb_mProcess, "setpriority", proc_setpriority.singleton, 3, null);

            Class.rb_define_module_function(rb_mProcess, "uid", proc_getuid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcess, "uid=", proc_setuid.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcess, "gid", proc_getgid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcess, "gid=", proc_setgid.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcess, "euid", proc_geteuid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcess, "euid=", proc_seteuid.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcess, "egid", proc_getegid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcess, "egid=", proc_setegid.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcess, "initgroups", proc_initgroups.singleton, 2, null);
            Class.rb_define_module_function(rb_mProcess, "groups", proc_getgroups.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcess, "groups=", proc_setgroups.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcess, "maxgroups", proc_getmaxgroups.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcess, "maxgroups=", proc_setmaxgroups.singleton, 1, null);

            Class.rb_define_module_function(rb_mProcess, "times", rb_proc_times.singleton, 0, null);

            S_Tms = Struct.rb_struct_define("Tms", null, "utime", "stime", "cutime", "cstime");

            //Process.SAVED_USER_ID = Win32.geteuid();
            //Process.SAVED_GROUP_ID = Win32.getegid();

            rb_mProcUID = Class.rb_define_module_under(rb_mProcess, "UID", null);
            rb_mProcGID = Class.rb_define_module_under(rb_mProcess, "GID", null);

            Class.rb_define_module_function(rb_mProcUID, "rid", proc_getuid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcGID, "rid", proc_getgid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcUID, "eid", proc_geteuid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcGID, "eid", proc_getegid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcUID, "change_privilege", p_uid_change_privilege.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcGID, "change_privilege", p_gid_change_privilege.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcUID, "grant_privilege", p_uid_grant_privilege.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcGID, "grant_privilege", p_gid_grant_privilege.singleton, 1, null);
            Class.rb_define_alias(rb_mProcUID, "eid=", "grant_privilege", null);
            Class.rb_define_alias(rb_mProcGID, "eid=", "grant_privilege", null);
            Class.rb_define_module_function(rb_mProcUID, "re_exchange", p_uid_exchange.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcGID, "re_exchange", p_gid_exchange.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcUID, "re_exchangeable?", p_uid_exchangeable.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcGID, "re_exchangeable?", p_gid_exchangeable.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcUID, "sid_available?", p_uid_have_saved_id.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcGID, "sid_available?", p_gid_have_saved_id.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcUID, "switch", p_uid_switch.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcGID, "switch", p_gid_switch.singleton, 0, null);

            rb_mProcID_Syscall = Class.rb_define_module_under(rb_mProcess, "Sys", null);

            Class.rb_define_module_function(rb_mProcID_Syscall, "getuid", proc_getuid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcID_Syscall, "geteuid", proc_geteuid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcID_Syscall, "getgid", proc_getgid.singleton, 0, null);
            Class.rb_define_module_function(rb_mProcID_Syscall, "getegid", proc_getegid.singleton, 0, null);

            Class.rb_define_module_function(rb_mProcID_Syscall, "setuid", p_sys_setuid.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcID_Syscall, "setgid", p_sys_setgid.singleton, 1, null);

            Class.rb_define_module_function(rb_mProcID_Syscall, "setruid", p_sys_setruid.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcID_Syscall, "setrgid", p_sys_setrgid.singleton, 1, null);

            Class.rb_define_module_function(rb_mProcID_Syscall, "seteuid", p_sys_seteuid.singleton, 1, null);
            Class.rb_define_module_function(rb_mProcID_Syscall, "setegid", p_sys_setegid.singleton, 1, null);

            Class.rb_define_module_function(rb_mProcID_Syscall, "setreuid", p_sys_setreuid.singleton, 2, null);
            Class.rb_define_module_function(rb_mProcID_Syscall, "setregid", p_sys_setregid.singleton, 2, null);

            Class.rb_define_module_function(rb_mProcID_Syscall, "setresuid", p_sys_setresuid.singleton, 3, null);
            Class.rb_define_module_function(rb_mProcID_Syscall, "setresgid", p_sys_setresgid.singleton, 3, null);
            Class.rb_define_module_function(rb_mProcID_Syscall, "issetugid", p_sys_issetugid.singleton, 0, null);
        }

        static void Init_load()
        {
            Eval.rb_load_path.value = new Array();
            Variables.rb_define_variable("$:", Eval.rb_load_path);
            Variables.rb_define_variable("$-I", Eval.rb_load_path);
            Variables.rb_define_variable("$LOAD_PATH", Eval.rb_load_path);

            Eval.rb_features.value = new Array();
            Variables.rb_define_variable("$\"", Eval.rb_features);
            Variables.rb_define_variable("$LOADED_FEATURES", Eval.rb_features);

            Class.rb_define_global_function("load", rb_f_load.singleton, -1, null);
            Class.rb_define_global_function("require", rb_f_require.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "autoload", rb_mod_autoload.singleton, 2, null);
            Class.rb_define_method(rb_cModule, "autoload?", rb_mod_autoload_p.singleton, 1, null);
            Class.rb_define_global_function("autoload", rb_f_autoload.singleton, 2, null);
            Class.rb_define_global_function("autoload?", rb_f_autoload_p.singleton, 1, null);
            //rb_global_variable(Eval.ruby_wrapper);

            //Eval.ruby_dln_librefs = new Array();
            //rb_global_variable(Eval.ruby_dln_librefs);

            ((Array)Eval.rb_load_path.value).Add(new String(@"."));
            string RUBYLIB = System.Environment.GetEnvironmentVariable("RUBYLIB");
            if (RUBYLIB != null)
                foreach (string path in RUBYLIB.Split(';'))
                    ((Array)Eval.rb_load_path.value).Add(new String(path.Trim()));

            string CLRpath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(string)).Location);
            ((Array)Eval.rb_load_path.value).Add(new String(CLRpath));
        }

        static void Init_Proc()
        {
            rb_eLocalJumpError = Class.rb_define_class("LocalJumpError", rb_eStandardError, null);
            Class.rb_define_method(rb_eLocalJumpError, "exit_value", localjump_xvalue.singleton, 0, null);
            Class.rb_define_method(rb_eLocalJumpError, "reason", localjump_reason.singleton, 0, null);

            //Eval.exception_error = new FatalException("exception reentered");
            //rb_global_variable(Eval.exception_error);

            rb_eSysStackError = Class.rb_define_class("SystemStackError", rb_eStandardError, null);
            //Eval.sysstack_error = new SysStackError("stack level too deep");
            //OBJ_TAINT(Eval.sysstack_error);
            //rb_global_variable(Eval.sysstack_error);

            rb_cProc = Class.rb_define_class("Proc", rb_cObject, null);
            Class.rb_undef_alloc_func(rb_cProc);
            Class.rb_define_singleton_method(rb_cProc, "new", proc_s_new.singleton, 1, null);

            Class.rb_define_method(rb_cProc, "clone", proc_clone.singleton, 0, null);
            Class.rb_define_method(rb_cProc, "dup", proc_dup.singleton, 0, null);
            Class.rb_define_method(rb_cProc, "call", proc_call.singleton, 2, null);
            Class.rb_define_method(rb_cProc, "arity", proc_arity.singleton, 0, null);
            Class.rb_define_method(rb_cProc, "[]", proc_call.singleton, 2, null);
            Class.rb_define_method(rb_cProc, "==", proc_eq.singleton, 1, null);
            Class.rb_define_method(rb_cProc, "to_s", proc_to_s.singleton, 0, null);
            Class.rb_define_method(rb_cProc, "to_proc", proc_to_self.singleton, 0, null);
            Class.rb_define_method(rb_cProc, "binding", proc_binding.singleton, 0, null);

            Class.rb_define_global_function("proc", proc_lambda.singleton, 0, null);
            Class.rb_define_global_function("lambda", proc_lambda.singleton, 0, null);

            rb_cMethod = Class.rb_define_class("Method", rb_cObject, null);
            Class.rb_undef_alloc_func(rb_cMethod);
            Class.rb_undef_method(Class.CLASS_OF(rb_cMethod), "new");
            Class.rb_define_method(rb_cMethod, "==", method_eq.singleton, 1, null);
            Class.rb_define_method(rb_cMethod, "clone", method_clone.singleton, 0, null);
            Class.rb_define_method(rb_cMethod, "call", method_call.singleton, 1, null);
            Class.rb_define_method(rb_cMethod, "[]", method_call.singleton, 1, null);
            Class.rb_define_method(rb_cMethod, "arity", method_arity.singleton, 0, null);
            Class.rb_define_method(rb_cMethod, "inspect", method_inspect.singleton, 0, null);
            Class.rb_define_method(rb_cMethod, "to_s", method_inspect.singleton, 0, null);
            Class.rb_define_method(rb_cMethod, "to_proc", method_proc.singleton, 0, null);
            Class.rb_define_method(rb_cMethod, "unbind", method_unbind.singleton, 0, null);
            Class.rb_define_method(rb_mKernel, "method", rb_obj_method.singleton, 1, null);

            rb_cUnboundMethod = Class.rb_define_class("UnboundMethod", rb_cObject, null);
            Class.rb_undef_alloc_func(rb_cUnboundMethod);
            Class.rb_undef_method(Class.CLASS_OF(rb_cUnboundMethod), "new");
            Class.rb_define_method(rb_cUnboundMethod, "==", method_eq.singleton, 1, null);
            Class.rb_define_method(rb_cUnboundMethod, "clone", method_clone.singleton, 0, null);
            Class.rb_define_method(rb_cUnboundMethod, "arity", method_arity.singleton, 0, null);
            Class.rb_define_method(rb_cUnboundMethod, "inspect", method_inspect.singleton, 0, null);
            Class.rb_define_method(rb_cUnboundMethod, "to_s", method_inspect.singleton, 0, null);
            Class.rb_define_method(rb_cUnboundMethod, "bind", umethod_bind.singleton, 1, null);
            Class.rb_define_method(rb_cModule, "instance_method", rb_mod_method.singleton, 1, null);
        }

        static void Init_Binding()
        {
            rb_cBinding = Class.rb_define_class("Binding", rb_cObject, null);
            Class.rb_undef_alloc_func(rb_cBinding);
            Class.rb_undef_method(Class.CLASS_OF(rb_cBinding), "new");
            Class.rb_define_method(rb_cBinding, "clone", proc_clone.singleton, 0, null);
            Class.rb_define_global_function("binding", rb_f_binding.singleton, 0, null);
        }

        static void Init_Math()
        {
            rb_mMath = Class.rb_define_module("Math", null);

            Variables.rb_define_const(rb_mMath, "PI", new Float(System.Math.Atan(1.0) * 4.0));
            Variables.rb_define_const(rb_mMath, "E", new Float(System.Math.Exp(1.0)));

            Class.rb_define_module_function(rb_mMath, "atan2", math_atan2.singleton, 2, null);
            Class.rb_define_module_function(rb_mMath, "cos", math_cos.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "sin", math_sin.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "tan", math_tan.singleton, 1, null);

            Class.rb_define_module_function(rb_mMath, "acos", math_acos.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "asin", math_asin.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "atan", math_atan.singleton, 1, null);

            Class.rb_define_module_function(rb_mMath, "cosh", math_cosh.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "sinh", math_sinh.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "tanh", math_tanh.singleton, 1, null);

            Class.rb_define_module_function(rb_mMath, "acosh", math_acosh.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "asinh", math_asinh.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "atanh", math_atanh.singleton, 1, null);

            Class.rb_define_module_function(rb_mMath, "exp", math_exp.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "log", math_log.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "log10", math_log10.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "sqrt", math_sqrt.singleton, 1, null);

            Class.rb_define_module_function(rb_mMath, "frexp", math_frexp.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "ldexp", math_ldexp.singleton, 2, null);

            Class.rb_define_module_function(rb_mMath, "hypot", math_hypot.singleton, 2, null);

            Class.rb_define_module_function(rb_mMath, "erf", math_erf.singleton, 1, null);
            Class.rb_define_module_function(rb_mMath, "erfc", math_erfc.singleton, 1, null);
        }

        static void Init_GC()
        {
            rb_mGC = Class.rb_define_module("GC", null);
            Class.rb_define_singleton_method(rb_mGC, "start", rb_gc_start.singleton, 0, null);
            Class.rb_define_singleton_method(rb_mGC, "enable", rb_gc_enable.singleton, 0, null);
            Class.rb_define_singleton_method(rb_mGC, "disable", rb_gc_disable.singleton, 0, null);
            Class.rb_define_method(rb_mGC, "garbage_collect", rb_gc_start.singleton, 0, null);

            rb_mObjectSpace = Class.rb_define_module("ObjectSpace", null);
            Class.rb_define_module_function(rb_mObjectSpace, "each_object", os_each_obj.singleton, 1, null);
            Class.rb_define_module_function(rb_mObjectSpace, "garbage_collect", rb_gc_start.singleton, 0, null);
            Class.rb_define_module_function(rb_mObjectSpace, "add_finalizer", add_final.singleton, 1, null);
            Class.rb_define_module_function(rb_mObjectSpace, "remove_finalizer", rm_final.singleton, 1, null);
            Class.rb_define_module_function(rb_mObjectSpace, "finalizers", finals.singleton, 0, null);
            Class.rb_define_module_function(rb_mObjectSpace, "call_finalizer", call_final.singleton, 1, null);

            Class.rb_define_module_function(rb_mObjectSpace, "define_finalizer", define_final.singleton, -1, null);
            Class.rb_define_module_function(rb_mObjectSpace, "undefine_finalizer", undefine_final.singleton, 1, null);

            Class.rb_define_module_function(rb_mObjectSpace, "_id2ref", id2ref.singleton, 1, null);

            //rb_gc_register_address(rb_mObSpace);
            //rb_global_variable(finalizers);
            //rb_gc_unregister_address(rb_mObSpace);
            //finalizers = new Array();

            // source_filenames = st_init_strtable();

            //nomem_error = new Exception(rb_eNoMemError, "failed to allocate memory");
            //rb_global_variable(nomem_error);
        }

        static void Init_marshal()
        {
            rb_mMarshal = Class.rb_define_module("Marshal", null);


            //currently defined in Marshal.c
            //s_dump = rb_intern("_dump");
            //s_load = rb_intern("_load");
            //s_mdump = rb_intern("marshal_dump");
            //s_mload = rb_intern("marshal_load");
            //s_dump_data = rb_intern("_dump_data");
            //s_load_data = rb_intern("_load_data");
            //s_alloc = rb_intern("_alloc");
            //s_getc = rb_intern("getc");
            //s_read = rb_intern("read");
            //s_write = rb_intern("write");
            //s_binmode = rb_intern("binmode");

            Class.rb_define_module_function(rb_mMarshal, "dump", marshal_dump.singleton, 1, null);
            Class.rb_define_module_function(rb_mMarshal, "load", marshal_load.singleton, 1, null);
            Class.rb_define_module_function(rb_mMarshal, "restore", marshal_load.singleton, 1, null);

            Variables.rb_define_const(rb_mMarshal, "MAJOR_VERSION", Marshal.MARSHAL_MAJOR);
            Variables.rb_define_const(rb_mMarshal, "MINOR_VERSION", Marshal.MARSHAL_MINOR);
        }

        static void Init_version()
        {
            object v = Object.rb_obj_freeze(null, new String(Version.ruby_version));
            object d = Object.rb_obj_freeze(null, new String(Version.ruby_release_date));
            object p = Object.rb_obj_freeze(null, new String(Version.ruby_platform));

            Variables.rb_define_global_const("RUBY_VERSION", v);
            Variables.rb_define_global_const("RUBY_RELEASE_DATE", d);
            Variables.rb_define_global_const("RUBY_PLATFORM", p);

            Variables.rb_define_global_const("VERSION", v);
            Variables.rb_define_global_const("RELEASE_DATE", d);
            Variables.rb_define_global_const("PLATFORM", p);
        }

        static void Init_Prog()
        {
            //init_ids();

            //ruby_sourcefile = rb_source_filename("ruby");
            Variables.rb_define_variable("$VERBOSE", Options.ruby_verbose);
            Variables.rb_define_variable("$-v", Options.ruby_verbose);
            Variables.rb_define_variable("$-w", Options.ruby_verbose);
            Variables.rb_define_variable("$DEBUG", Options.ruby_debug);
            Variables.rb_define_variable("$-d", Options.ruby_debug);
            Variables.rb_define_variable("$-p", Options.do_print);
            Variables.rb_define_variable("$-l", Options.do_line);

            Variables.rb_define_variable("$0", Options.rb_progname);
            Variables.rb_define_variable("$PROGRAM_NAME", Options.rb_progname);

            Options.rb_argv.value = new Array();
            Variables.rb_define_variable("$*", Options.rb_argv);
            Variables.rb_define_global_const("ARGV", Options.rb_argv.value);
            Variables.rb_define_variable("$-a", Options.do_split);
            //rb_global_variable(&rb_argv0);
        }
    }
}