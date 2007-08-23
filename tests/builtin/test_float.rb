require 'test/unit'

class TestNumeric < Test::Unit::TestCase
  def setup
    @fix = 5
    @big = 20 ** 20
  end

  def test_ceil_bignum
    i = (@big - 0.9).ceil
    assert_equal(@big, i)
    assert_instance_of(Bignum, i)

    b = -@big
    assert_equal(b, (b - 0.9).ceil)
  end

  def test_floor_bignum
    i = (@big + 0.9).floor
    assert_equal(@big, i)
    assert_instance_of(Bignum, i)

    b = -@big
    assert_equal(b, (b + 0.9).floor)
  end

  def test_infinite_eh
    inf = 1.0/0.0
    ninf = (-1.0)/0.0
    assert_equal(1, inf.infinite?)
    assert_equal(-1, ninf.infinite?)
    assert_equal(nil, 1.5.infinite?)
  end

  def test_mul_fixnum
    assert_equal(7.5, 2.5 * 3)
    assert_instance_of(Float, 2.0 * 5)
  end

  def test_pow_fixnum
    assert_equal(4.41, 2.1 ** 2)
    assert_instance_of(Float, 2.0 ** 2)
  end

  def test_round
    i = 1.0.round
    assert_equal(1, 0.5.round)
    assert_equal(1, i)
    assert_equal(1, 1.4.round)
    assert_instance_of(Fixnum, i)

    assert_equal(-1, (-0.5).round)
    assert_equal(-1, (-1.0).round)
    assert_equal(-1, (-1.4).round)

    i = (1.0 * @big).round
    assert_equal(@big, (@big - 0.5).round)
    assert_equal(@big, i)
    assert_equal(@big, (@big + 0.4).round)
    assert_instance_of(Bignum, i)

    b = -@big
    assert_equal(b, (b - 0.4).round)
    assert_equal(b, (1.0 * b).round)
    assert_equal(b, (b + 0.5).round)
  end

  def test_to_f
    assert_equal(1.2, 1.2.to_f)
    assert_instance_of(Float, 1.0.to_f)
  end

  def test_to_i
    util_truncate(:to_i)
  end

  def test_to_int
    util_truncate(:to_int)
  end

  def test_truncate
    util_truncate(:truncate)
  end

  def test_zero_eh
    assert_equal(true, 0.0.zero?)
    assert_equal(false, 1.0.zero?)
  end

  # Utilities

  def util_truncate(meth)
    i = (@big + 0.9).send(meth)
    assert_equal(@big, i)
    assert_instance_of(Bignum, i)

    b = -@big
    assert_equal(b, (b - 0.9).send(meth))
  end

end
