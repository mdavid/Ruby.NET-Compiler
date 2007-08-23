require 'test/unit'

class TestFixnum < Test::Unit::TestCase
  def setup
    @fix = 5
  end

  def test_aref
    # There is no Fixnum#[]=
    assert_raise(NoMethodError) { 2[0] = 1 }
  end

  def test_div_use_correct_method_if_coerced
    assert_equal('/', 1 / CoerceToDivSlash.new)
    assert_equal('div', 1.div(CoerceToDivSlash.new))
  end

  def test_div_zero_division
    util_zero_division_error(:/)
    util_zero_division_no_error(:/) {|x| x.infinite? == 1}

    util_zero_division_error(:div)
    util_zero_division_float_domain_error(:div)
  end

  def test_divmod_zero_division
    util_zero_division_error(:divmod)
    # TODO: In future Ruby versions, this changes to:
    # 1.divmod(0.0) #=> FloatDomainError
    util_zero_division_no_error(:divmod) {|x|
      x[0].nan? && x[1].nan?
    }
  end

  def test_id2name
    # Requires Symbol#to_i
    id = :@test.to_i
    assert_equal('@test', id.id2name)
  end

  def test_lshift_negative
    assert_equal(12, 3 << 2)
  end

  def test_modulo_zero_division
    util_zero_division_error(:%)
    util_zero_division_no_error(:%) {|x| x.nan?}

    util_zero_division_error(:modulo)
    util_zero_division_no_error(:modulo) {|x| x.nan?}
  end

  def test_quo
    res = 3.quo(1)
    assert_equal(3.0, res)
    assert_instance_of(Float, res)

    res = 3.quo(1.5)
    assert_equal(2.0, res)
    assert_instance_of(Float, res)

    assert_equal(1.2, 3.quo(2.5))

    util_zero_division_no_error(:quo) {|x| x.infinite? == 1}
  end

  def test_rev
    assert_equal(-6, ~5)
    assert_equal(4, ~(-5))
    assert_equal(-1, ~0)
  end

  def test_rshift
    # shift < 0
    assert_equal(12, 3 >> (-2))

    # shift == 0
    assert_equal(3, 3 >> 0)

    # shift too big for int
    assert_equal(-1, -2 >> 1000)
    assert_equal(0, 1 >> 1000)

    # otherwise
    assert_equal(-1, -2 >> 2)
    assert_equal(2, 10 >> 2)
    assert_equal(0, 1 >> 2)
  end

  def test_to_s_2
    # Special case in MRI
    assert_equal('10', 2.to_s(2))
  end

  def test_to_s_illegal_radix
    assert_raise(ArgumentError) { @fix.to_s(1) }
    assert_raise(ArgumentError) { @fix.to_s(37) }
  end

  def test_to_sym
    # Requires Symbol#to_i
    id = :@test.to_i
    assert_equal(:@test, id.to_sym)
  end

  def test_xor
    assert_equal(6, 11 ^ 13)
    assert_equal(-8, (-11) ^ 13)
    assert_equal(6, (-11) ^ (-13))
  end

  def test_zero_eh
    assert_equal(true, 0.zero?)
    assert_equal(false, 1.zero?)
    assert_equal(false, (-1).zero?)
  end

  # Utilities

  class DivSlash
    def /(y)
      '/'
    end
    def div(y)
      'div'
    end
  end

  class CoerceToDivSlash
    def coerce(x)
      [DivSlash.new, DivSlash.new]
    end
  end

  # TODO: also used in bignum.c (bigdivrem)
  def util_zero_division_error(meth)
    assert_raise(ZeroDivisionError) { @fix.send(meth, 0) }
  end

  def util_zero_division_float_domain_error(meth)
    assert_raise(FloatDomainError) { @fix.send(meth, 0.0) }
  end

  def util_zero_division_no_error(meth)
    res = nil
    assert_nothing_raised { res = @fix.send(meth, 0.0) }
    assert(yield(res))
  end

end
