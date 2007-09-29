require 'test/unit'

# Note:
# - The STR_ASSOC flag is helper for pack.c (Array#pack and String#unpack).
#   Associated strings cannot be gc'ed. This probably needs to be tested.
# - string_value is helper for a lot of things, used everywhere.
#   In particular, StringValueCStr raises ArgumentError if the string contains NULL.
# - uscore_get returns $_, raises TypeError if $_ is not (kind/instance?) string.
#   Used by Kernel::{chomp, chomp!, chop, chop!, gsub, gsub!, scan, split, sub, sub!}.
#   Put in test_kernel?

class TestString < Test::Unit::TestCase
  # TODO:
  # - rb_str_aref near end (GOTO): is this not covered?
  # - rb_str_splice
  # - rb_str_aset: default case - converted into id
  # - rb_str_reverse: special case when length == 0; should we test this?
  # - rb_str_to_i calls:
  #   - bignum.c->rb_str_to_inum. Missed: if(s[len]) /*no sentinel*/
  #   - bignum.c->rb_cstr_to_inum. Missed:
  #     badcheck and ISSPACE(*str) (#337), if /\+\+/ and badcheck (#351),
  #     *end == '_' goto bigparse (#431), if (badcheck) (#432),
  #     if (badcheck && *str == '_') (#453),
  #     if (c == '_') (#459), !ISASCII (#466), if (badcheck) { str--... } (#498)
  # - Check rb_str_to_f as well.

  def test_to_i
    # With spaces these become weird
    assert_equal(0, '++5'.to_i)
    assert_equal(0, '+-5'.to_i)
    assert_equal(0, '-+5'.to_i)
    assert_equal(0, '--5'.to_i)

    assert_equal(2, '+010'.to_i(2))
    assert_equal(-2, '-010'.to_i(2))
    assert_equal(2, '+0b010'.to_i(0))
    assert_equal(2, '+0b010'.to_i(2))
    assert_equal(2, '+0B010'.to_i(0))
    assert_equal(2, '+0B010'.to_i(2))

    # Special case in MRI
    assert_equal(3, '+010'.to_i(3))
    assert_equal(-3, '-010'.to_i(3))

    assert_equal(8, '+010'.to_i(8))
    assert_equal(-8, '-010'.to_i(8))
    assert_equal(8, '+0o010'.to_i(0))
    assert_equal(8, '+0o010'.to_i(8))
    assert_equal(8, '+0O010'.to_i(0))
    assert_equal(8, '+0O010'.to_i(8))

    assert_equal(8, '010'.to_i(0))
    assert_equal(8, '+010'.to_i(0))
    assert_equal(-8, '-010'.to_i(0))

    assert_equal(10, '+010'.to_i)
    assert_equal(10, '+10'.to_i(0))
    assert_equal(10, '+010'.to_i(10))
    assert_equal(10, '  +010'.to_i)
    assert_equal(-10, '-010'.to_i)
    assert_equal(-10, '-10'.to_i(0))
    assert_equal(-10, '-010'.to_i(10))
    assert_equal(-10, '  -010'.to_i)
    assert_equal(10, '+0d010'.to_i(0))
    assert_equal(10, '+0d010'.to_i(10))
    assert_equal(10, '+0D010'.to_i(0))
    assert_equal(10, '+0D010'.to_i(10))

    assert_equal(16, '+010'.to_i(16))
    assert_equal(-16, '-010'.to_i(16))
    assert_equal(16, '+0x010'.to_i(0))
    assert_equal(16, '+0x010'.to_i(16))
    assert_equal(16, '+0X010'.to_i(0))
    assert_equal(16, '+0X010'.to_i(16))

    # Special case in MRI
    assert_equal(32, '+010'.to_i(32))
    assert_equal(-32, '-010'.to_i(32))

    assert_raise(ArgumentError) { '0'.to_i(1) }
    assert_nothing_raised { '0'.to_i(36) }
    assert_raise(ArgumentError) { '0'.to_i(37) }
    assert_raise(ArgumentError) { '0'.to_i(38) }
  end

  def setup
    @hello = 'Hello'
  end

  def test_aref_arity
    util_slice_arity(:[])
  end

  def test_aset_string_no_match
    assert_raise(IndexError) { @hello['a'] = 'z' }
    assert_raise(IndexError) { @hello['hey'] = 'z' }
  end

  def test_aset_arity
    # 1 or 2 (minus the value)
    assert_raise(ArgumentError) { @hello[] = 'a' }
    assert_raise(ArgumentError) { @hello[1, 2, 3] = 'a' }
    assert_raise(ArgumentError) { @hello[1, 2, 3, 4] = 'a' }
  end

  def test_cmp_nonstring
    # Must respond to to_str
    assert_not_equal(0, @hello.<=>(NoToStr.new))
    # Must respond to <=>
    assert_not_equal(0, @hello.<=>(NoCmp.new))
    # If the result is nil, return nil
    assert_nil(@hello.<=>(CmpReturnsNil.new))
    # If the result is fixnum, return -fixnum
    assert_equal(-1, @hello.<=>(CmpReturnsOne.new))
    # Otherwise, return 0.-(result)
    # TODO
  end

  # TODO: rb_str_cmp is similar to this
  def test_eql_eh_false
    assert(!@hello.eql?('h'))
    assert(!@hello.eql?('hello')) # wrong case
    assert(!@hello.eql?('helloo'))
    assert(!@hello.eql?(Array.new))
  end

  def test_equals2_arg_has_to_str
    # Note:
    # The MRI code documentation is wrong. It says "if obj is not a String,
    # returns false". The code actually considers obj a String if it responds to
    # to_str. This test assumes MRI-like behaviour.

    # "true" ensures that the result is boolean; ".==" ensures call order
    assert_equal(true, @hello.==(HasToStrAndEql2.new))
  end

  #def test_gsub_cheating_block
  #  util_gsub_cheating_block(:gsub)
  #end

  def test_gsub_arity
    util_gsub_arity(:gsub)
  end

  def test_gsub_with_block
    # Test match in the middle of string.
    s = 'a b c'
    s1 = s.gsub(/ /) {|s| '' }
    assert_equal('abc', s1)
  end

  #def test_gsub_bang_cheating_block
  #  util_gsub_cheating_block(:gsub!)
  #end

  def test_gsub_bang_arity
    util_gsub_arity(:gsub!)
  end

  def test_index_sub_nonstring
    # Must respond to to_str
    assert_raise(TypeError) { @hello.index(NoToStr.new) }
    # to_str must return string
    assert_raise(TypeError) { @hello.index(ToStrReturnsArray.new, 0) }
    assert_nothing_raised { @hello.index(ToStrReturnsYes.new, 0) }
  end

  def test_rindex_invalid_sub
    assert_raise(TypeError) { @hello.rindex(Array.new) }
  end

  def test_rindex_negative_position
    assert_equal(4, @hello.rindex('o', -1))
    assert_equal(nil, @hello.rindex('o', -2))
    assert_equal(0, @hello.rindex('H', -5))
    assert_equal(1, @hello.rindex('e', -2))
    assert_equal(nil, @hello.rindex('.', -1)) # not regexp

    'a' =~ /(a)/ # simulate $1 = 0
    $~ = nil
    res = @hello.rindex(/./, -1)
    assert_equal(4, res)
    assert_instance_of(MatchData, $~)
    assert_nil($1)

    'a' =~ /(a)/ # simulate $1 = 0
    $~ = nil
    res = @hello.rindex(/(.)/, -1)
    assert_instance_of(MatchData, $~)
    assert_equal('o', $1)

    # Too small

    assert_equal(nil, @hello.rindex('H', -6))

    'a' =~ /(a)/ # simulate $1 = 0
    res = @hello.rindex(/(.)/, -6)
    assert_nil(res)
    assert_nil($~)
    assert_nil($1)
  end

  def test_slice_arity
    util_slice_arity(:slice)
  end

  def test_slice_bang_arity
    # 1 or 2
    assert_raise(ArgumentError) { @hello.slice! }
    assert_raise(ArgumentError) { @hello.slice!(1, 2, 3) }
    assert_raise(ArgumentError) { @hello.slice!(1, 2, 3, 4) }
  end

  def test_sub_bang_arity
    # 1 + block, or 2
    assert_raise(ArgumentError) { @hello.sub! }
    assert_raise(ArgumentError) { @hello.sub!(1) }
    assert_raise(ArgumentError) { @hello.sub!(1, 2, 3) }
    assert_raise(ArgumentError) { @hello.sub!(1, 2, 3, 4) }
  end

  def test_to_i_negative_base
    assert_raise(ArgumentError) { '123'.to_i(-1) }
  end

  def test_upto_length_increase_beyond_end
    i = 0
    WeirdSucc.new('Hello').upto('Hellw') do |s|
      case i
        when 0
          assert_equal(s, 'Hello')
        when 1
          assert_equal(s, 'Hellp')
        when 2
          assert_equal(s, 'Hellq')
        else
          flunk
      end
      i += 1
    end
    assert_equal(3, i)
  end

  # Utilities

  class ToStrReturnsYes
    def to_str
      'Yes'
    end
  end

  class ToStrReturnsArray
    def to_str
      Array.new
    end
  end

  class HasToStrAndEql2
    def to_str
    end
    def ==(other)
      'Yes'
    end
    def <=>(other) # must not use this
      'No'
    end
  end

  class CmpReturnsOne
    def to_str
    end
    def <=>(other)
      1
    end
  end

  class CmpReturnsNil
    def to_str
    end
    def <=>(other)
      nil
    end
  end

  class CmpReturnsYes
    def to_str
    end
    def <=>(other)
      'Yes'
    end
  end

  class NoCmp
    def to_str
    end
  end

  class NoToStr
    def ==(other)
      true
    end
  end

  class WeirdSucc < String
    def <=>(other)
      super(other)
    end
    def succ
      if self == 'Hellq' then
        return WeirdSucc.new('Hellrr')
      end
      WeirdSucc.new(super)
    end
  end

  def util_gsub_arity(meth)
    # 1 + block, or 2
    assert_raise(ArgumentError) { @hello.send(meth) }
    assert_raise(ArgumentError) { @hello.send(meth, 'e') }
    assert_raise(ArgumentError) { @hello.send(meth, 'e', 'a', 3) }
    assert_raise(ArgumentError) { @hello.send(meth, 'e', 'a', 3, 4) }
  end

  ### FIXME: No exception, no coredump, what's wrong?
  #def util_gsub_cheating_block(meth)
  #  # MRI coredump bug
  #  # http://blade.nagaokaut.ac.jp/cgi-bin/scat.rb/ruby/ruby-dev/24827
  #  assert_raise(RuntimeError) do
  #    str = "a" * 0x20000
  #    str.gsub(/\z/) {
  #      dest = nil
  #      ObjectSpace.each_object(String) {|o|
  #        dest = o if o.length == 0x20000+30
  #      }
  #      dest
  #    }
  #  end
  #end

  def util_slice_arity(meth)
    # 1 or 2
    assert_raise(ArgumentError) { @hello.send(meth) }
    assert_raise(ArgumentError) { @hello.send(meth, 1, 2, 3) }
    assert_raise(ArgumentError) { @hello.send(meth, 1, 2, 3, 4) }
  end

end
