# TODO: This is completely unfinished.

require 'test/unit'

class TestSymbol < Test::Unit::TestCase
  # Requires String#intern.

  def setup
    @sym = "Hello!".intern
  end

  def test_inspect_can_be_evaluated
    # Whatever the internal representation is, `sym.inspect'
    # "returns the representation of sym as a symbol literal",
    # which means that eval() must reproduce the original symbol.
    # This test also checks that weird characters are handled correctly.
    sym = "I\3...\nU!".intern
    assert_same(sym, eval(sym.inspect))
  end

  def test_to_i
    assert_kind_of(Integer, @sym.to_i)

    # These may have different IDs but must produce the same symbol number.
    sym1 = "a".intern
    sym2 = "a".intern
    assert_equal(sym1.to_i, sym2.to_i)
  end

end
