require 'test/unit'

# TODO: num_remainder

class TestNumeric < Test::Unit::TestCase
  def setup
    @fix = 5
    @flo = 5.5
    @big = 20 ** 20
    @my = MyNumeric.new
  end

  # TODO: Maybe try to coerce with invalid args?
  def test_coerce
    assert_equal([1, 2], 2.coerce(1))
    assert_equal([1.5, 2.0], 2.coerce(1.5))
    assert_equal([1.0, 2.5], 2.5.coerce(1))
    assert_equal([1.0, 2.5], 2.5.coerce(1.0))

    res = @big.coerce(@fix)
    assert_instance_of(Bignum, res[0])
    assert_instance_of(Bignum, res[1])

    res = @fix.coerce(@big)
    assert_instance_of(Float, res[0])
    assert_instance_of(Float, res[1])

    res = @flo.coerce(@big)
    assert_instance_of(Float, res[0])
    assert_instance_of(Float, res[1])

    assert_raise(TypeError) { @big.coerce(@flo) }
  end

  def test_div
    assert_equal(10, @my.div(1))
  end

  def test_divmod
    assert_equal([10, 20.9], @my.divmod(1))
  end

  def test_integer_eh
    assert_equal(false, @my.integer?)
  end

  def test_modulo
    assert_equal(20.9, @my.modulo(1))
  end

  def test_quo
    assert_equal(10.9, @my.quo(1))
  end

  def test_to_int
    assert_equal('i', @my.to_int)
  end

  # FIXME: Doesn't cover num_uminus
  #~ def test_uminus
    #~ assert_equal(0.-(@fix), -@fix)
    #~ assert_equal(0.-(@flo), -@flo)
    #~ assert_equal(0.-(@big), -@big)
  #~ end

  def test_uplus
    assert_equal(@fix, +@fix)
    assert_equal(@flo, +@flo)
    assert_equal(@big, +@big)
  end

  # Utilities

  class MyNumeric < Numeric
    def /(other)
      10.9
    end
    def %(other)
      20.9
    end
    def to_i
      'i'
    end
  end

end
