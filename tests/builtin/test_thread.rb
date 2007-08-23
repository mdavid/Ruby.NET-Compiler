# TODO: This is completely unfinished.

require 'test/unit'

class TestThread < Test::Unit::TestCase
  def test_keys
    # When thread locals is empty, Thread#key?('') returns false.
    th = Thread.new {}
    assert_equal(false, th.key?(''))

    # When thread locals is not empty, Thread#key?('') throws exception.
    th = Thread.new { Thread.current[:a] = 'a' }
    assert_raise(ArgumentError) { th.key?('') }
  end

end
