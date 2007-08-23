require 'test/unit'

class TestString < Test::Unit::TestCase
  def setup
    @obj = Object.new
  end

  def test_respond_to_eh
    # String
    assert(@obj.respond_to?('respond_to?'))
    assert(!@obj.respond_to?('foo'))
    assert_raise(ArgumentError) { @obj.respond_to?('') }

    # Fixnum
    # Should warn as well.
    assert(@obj.respond_to?(:respond_to?.to_i))
    assert(!@obj.respond_to?(:foo.to_i))
    assert_raise(ArgumentError) {
      @obj.respond_to?(123456789) # Hopefully this is not a valid ID.
    }

    # Symbol
    assert(@obj.respond_to?(:respond_to?))
    assert(!@obj.respond_to?(:foo))

    # Other
    assert_raise(TypeError) { @obj.respond_to?(@obj) }
  end

end
