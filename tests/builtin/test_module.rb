require 'test/unit'

class TestModule < Test::Unit::TestCase
  def test_alias_method
    # TODO: Test when method is singleton.
    #       How? alias_method(:a, :b) doesn't work if b is singleton method.

    AliasModule.do_alias_method_x_1
    assert_equal(1, AliasModule.x)

    # XXX If verbose, this prints warning.
    AliasModule.do_alias_method_x_2
    assert_equal(2, AliasModule.x)

    # Also accepts Fixnum (with warning), String, and anything with to_str.
    # TODO: Is Fixnum considered part of the spec?

    assert_nothing_raised { AliasModule.do_alias_method_x_1_fix }
    assert_nothing_raised { AliasModule.do_alias_method_x_1_str }
    assert_nothing_raised { AliasModule.do_alias_method_x_1_to_str }

    assert_raise(TypeError) { AliasModule.do_alias_method_x_1_flo }
  end

  # Utilities

  module AliasModule
    module_function

    def one
      1
    end
    def two
      2
    end

    def do_alias_method_x_1
      alias_method(:x, :one)
      module_function :x
    end
    def do_alias_method_x_2
      alias_method(:x, :two)
      module_function :x
    end

    def do_alias_method_x_1_fix
      alias_method(:x.to_i, :one.to_i)
      module_function :x
    end
    def do_alias_method_x_1_flo
      alias_method(:x.to_i.to_f, :one.to_i.to_f)
      module_function :x
    end
    def do_alias_method_x_1_str
      alias_method('x', 'one')
      module_function :x
    end
    def do_alias_method_x_1_to_str
      alias_method(X.new, One.new)
      module_function :x
    end

    class X
      def to_str
        'x'
      end
    end
    class One
      def to_str
        'one'
      end
    end
  end

end
