require 'test/unit'

class TestMath < Test::Unit::TestCase
  EPSILON = 1e-7

  def test_acos
    assert_in_delta(1.0471975511966, Math.acos(0.5), EPSILON)
  end

  def test_acosh
    assert_in_delta(4.60507017098476, Math.acosh(50), EPSILON)
  end

  def test_asin
    assert_in_delta(0.523598775598299, Math.asin(0.5), EPSILON)
  end

  def test_asinh
    assert_in_delta(4.60527017099142, Math.asinh(50), EPSILON)
  end

  def test_atan
    assert_in_delta(1.55079899282175, Math.atan(50), EPSILON)
  end

  def test_atan2
    assert_in_delta(0.463647609000806, Math.atan2(50, 100), EPSILON)
  end

  def test_atanh
    assert_in_delta(0.549306144334055, Math.atanh(0.5), EPSILON)
  end

  def test_cos
    assert_in_delta(0.964966028492113, Math.cos(50), EPSILON)
  end

  def test_cosh
    assert_in_delta(1.12762596520638, Math.cosh(0.5), EPSILON)
  end

  def test_e
    assert_in_delta(2.71828182845905, Math::E, EPSILON)
  end

  def test_erf
    assert_in_delta(0.520499877813046, Math.erf(0.5), EPSILON)
  end

  def test_erfc
    assert_in_delta(0.479500122186954, Math.erfc(0.5), EPSILON)
  end

  def test_exp
    assert_in_delta(1.64872127070013, Math.exp(0.5), EPSILON)
  end

  def test_frexp
    res = Math.frexp(50)
    assert_in_delta(0.78125, res[0], EPSILON)
    assert_equal(6, res[1])
    assert_instance_of(Fixnum, res[1])
  end

  def test_hypot
    assert_in_delta(111.803398874989, Math.hypot(50, 100), EPSILON)
  end

  def test_ldexp
    assert_in_delta(0.5, Math.ldexp(0.5, 0.5), EPSILON)
    assert_in_delta(12.8, Math.ldexp(0.4, 5), EPSILON)
  end

  def test_log
    assert_in_delta(3.91202300542815, Math.log(50), EPSILON)
  end

  def test_log10
    assert_in_delta(1.69897000433602, Math.log10(50), EPSILON)
  end

  def test_pi
    assert_in_delta(3.14159265358979, Math::PI, EPSILON)
  end

  def test_sin
    assert_in_delta(-0.262374853703929, Math.sin(50), EPSILON)
  end

  def test_sinh
    assert_in_delta(0.521095305493747, Math.sinh(0.5), EPSILON)
  end

  def test_sqrt
    assert_in_delta(7.07106781186548, Math.sqrt(50), EPSILON)
  end

  def test_tan
    assert_in_delta(-0.271900611997631, Math.tan(50), EPSILON)
  end

  def test_tanh
    assert_in_delta(0.46211715726001, Math.tanh(0.5), EPSILON)
  end

end
