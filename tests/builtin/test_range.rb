require 'test/unit'

class TestRange < Test::Unit::TestCase
  def test_each_custom
    r = Range.new(FixnumWrapper.new(2), FixnumWrapper.new(5))
    res = 0
    r.each {|x| res += x.n }
    assert_equal(14, res)
  end

  def test_each_returns_self
    util_returns_self(:each)
  end

  def test_initialize_bad_value
    # (beg <=> end) must not be nil.
    assert_raise(ArgumentError) { Range.new(5, '5') }
    assert_raise(ArgumentError) { Range.new(CmpNil.new, CmpNil.new) }

    # (beg > end) is fine.
    assert_nothing_raised { Range.new(5, 4) }
    # String is fine.
    assert_nothing_raised { Range.new('a', 'f') }
    # Other classes are fine.
    assert_nothing_raised { Range.new(FixnumWrapper.new(1), FixnumWrapper.new(2)) }

    # The exception raised must be ArgumentError ...
    assert_raise(ArgumentError) {
      Range.new(CmpZeroDivision.new, CmpZeroDivision.new)
    }

    # ... unless it's not a StandardError.
    assert_raise(NotImplementedError) {
      Range.new(CmpNotImplemented.new, CmpNotImplemented.new)
    }
  end

  def test_step_nonpositive
    r = Range.new(1, 2)
    assert_raise(ArgumentError) { r.step(-1) {} }
    # This is tested earlier than "no block given".
    assert_raise(ArgumentError) { r.step(-1) }
    assert_raise(ArgumentError) { r.step(0) }

    # For some reason, MRI checks step(0) separately.

    r = Range.new('a', 'f')
    assert_raise(ArgumentError) { r.step(0) }

    r = Range.new(1.2, 2.2)
    assert_raise(ArgumentError) { r.step(0) }

    r = Range.new(FixnumWrapper.new(1), FixnumWrapper.new(2))
    assert_raise(ArgumentError) { r.step(0) }
  end

  def test_step_not_fixnum
    # TODO: Test edge cases in range_each_func.

    r = Range.new('a', 'f')
    res = ''
    r.step(2) {|x| res << x }
    assert_equal('ace', res)

    r = Range.new(1.2, 7.5)
    res = 0
    r.step(2) {|x| res += x }
    assert_equal(16.8, res)

    r = Range.new(FixnumWrapper.new(2), FixnumWrapper.new(5))
    res = 0
    r.step(2) {|x| res += x.n }
    assert_equal(6, res)
  end

  def test_step_returns_self
    util_returns_self(:step)
  end

  # Utilities

  class FixnumWrapper
    attr_reader :n
    def initialize(n)
      @n = n
    end
    def succ
      FixnumWrapper.new(@n + 1)
    end
    def <=>(other)
      @n <=> other.n
    end
  end

  class CmpNil
    def <=>(other)
      nil
    end
  end

  class CmpZeroDivision
    def <=>(other)
      raise ZeroDivisionError
    end
  end

  class CmpNotImplemented
    def <=>(other)
      raise NotImplementedError
    end
  end

  def util_returns_self(meth)
    r = Range.new(1, 2)
    r2 = r.send(meth) {}
    assert_same(r, r2)
  end

end
