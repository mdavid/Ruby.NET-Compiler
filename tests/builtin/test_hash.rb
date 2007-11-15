# Next: Hash#invert

require 'test/unit'

class TestHash < Test::Unit::TestCase
  def setup
    @h = {'a'=>5, 0=>'b'}
  end

  def test_aref_arity
    # 1
    assert_raise(ArgumentError) { @h[] }
    assert_raise(ArgumentError) { @h['a', 'b'] }
  end

  def test_aset
    assert_nothing_raised { @h[4] = 5 }
    assert_nothing_raised { @h[:a] = 5 }
    assert_nothing_raised { @h['a'] = 5 }
    # The key's hash method must return an object that responds to %,
    # even if the value returned by that % is not a Fixnum.
    assert_nothing_raised { @h[BignumHash.new] = 5 }
    assert_nothing_raised { @h[HasPercentHash.new] = 5 }
    assert_raise(NoMethodError) { @h[NoPercentHash.new] = 5 }
    assert_raise(NoMethodError) { @h[NilHash.new] = 5 }
  end

  def test_aset_arity
    # 2
    assert_raise(ArgumentError) { @h.[]= }
    assert_raise(ArgumentError) { @h.[]=('a') }
    assert_raise(ArgumentError) { @h.[]=('a', 'b', 'c') }
  end

  def test_clear_works_on_self_returns_self
    h = @h
    assert_same(h, @h.clear)
    assert_same(h, @h)
  end

  def test_default
    h = Hash.new {|h, k| k.inspect }
    # TODO: In future Ruby versions, this changes to:
    # h.default == nil
    assert_equal('nil', h.default)
    assert_equal('1', h.default(1))
    assert_equal({}, h)
  end

  def test_default_arity
    # 0 or 1
    assert_raise(ArgumentError) { @h.default(1, 2) }
  end

  def test_default_equals_with_proc
    # Check that the programmer can read--the documentation says "it is not
    # possible" instead of "it is possible".
    h = { "a" => 100, "b" => 200 }
    h.default = proc do |hash, key|
      hash[key] = key + key
    end
    assert_instance_of(Proc, h[2])
  end

  def test_default_proc_is_a_proc
    h = Hash.new {|h, k| k }
    assert_instance_of(Proc, h.default_proc)
  end

  def test_default_proc_not_provided
    h = Hash.new(1)
    assert_nil(h.default_proc)
  end

  def test_default_no_default
    # Documentation bug: if key is not found and the block is not supplied,
    # return nil, not any "default value".
    h = Hash.new('default')
    assert_nil(h.delete(1))
  end

  def test_delete_arity
    # 1
    assert_raise(ArgumentError) { @h.delete }
    assert_raise(ArgumentError) { @h.delete(1, 2) }
  end

  def test_each_returns_self
    assert_same(@h, @h.each {})
  end

  def test_each_with_single_param_block
    @h.each do |p|
      assert_instance_of(Array, p)
      assert_equal(2, p.length)
    end
  end

  def test_each_key_returns_self
    assert_same(@h, @h.each_key {})
  end

  def test_each_pair_returns_self
    assert_same(@h, @h.each_pair {})
  end

  def test_each_value_allows_modification
    assert_nothing_raised { @h.each_value {|v| @h['a'] = 10 } }
  end

  def test_each_value_returns_self
    assert_same(@h, @h.each_value {})
  end

  def test_empty_eh_arity
    util_0_arity(:empty?)
  end

  def test_empty_returns_boolean
    assert_equal(true, {}.empty?)
    assert_equal(false, {1=>2}.empty?) # must not be nil
  end

  def test_equal2_does_not_compare_default
    h1 = Hash.new(5)
    h2 = Hash.new(6)
    assert(h1 == h2) # Not using assert_equal, to make sure it uses Hash#==.

    h1 = Hash.new {|k| 5}
    h2 = Hash.new {|k| 6}
    assert(h1 == h2) # Not using assert_equal, to make sure it uses Hash#==.
  end

  def test_equal2_uses_correct_methods
    # Documentation bug: keys are compared using normal key equality checks
    # (eql? and hash), not using ==.
    h1 = {AlwaysEqualKey.new=>AlwaysEqualValue.new, AlwaysEqualKey.new=>AlwaysEqualValue.new}
    h2 = {AlwaysEqualKey.new=>AlwaysEqualValue.new, AlwaysEqualKey.new=>AlwaysEqualValue.new}
    assert(h1 == h2) # Not using assert_equal, to make sure it uses Hash#==.
  end

  def test_fetch_arity
    # 1 +/- block or 2 +/- block
    h = {1=>2, 3=>4, 5=>6}
    assert_raise(ArgumentError) { h.fetch }
    assert_raise(ArgumentError) { h.fetch {} }
    assert_raise(ArgumentError) { h.fetch(1, 3, 5) }
    assert_raise(ArgumentError) { h.fetch(1, 3, 5) {} }

    # TODO: Check that this produces warning.
    old_w = $-w
    $-w = nil  # Suppress warning.
    assert_nothing_raised { h.fetch(1, 3) {} }
    $-w = old_w
  end

  def test_initialize_arity
    # 0 - block, 0 + block, or 1 - block
    assert_raise(ArgumentError) { Hash.new(5) {} }
    assert_raise(ArgumentError) { Hash.new(5, 6) }
    assert_raise(ArgumentError) { Hash.new(5, 6) {} }
  end

  def test_inspect_recursive
    h = {}
    h[{h=>h}] = {h=>h}
    assert_equal('{{{...}=>{...}}=>{{...}=>{...}}}', h.inspect)
  end

  def test_length_arity
    util_0_arity(:length)
  end

  def test_rehash_during_iteration
    # Documentation bug: RuntimeError, not IndexError.

    assert_raise(RuntimeError)  { @h.each {|k, v| @h.rehash } }
    assert_raise(RuntimeError)  { @h.each_key {|k| @h.rehash } }
    assert_raise(RuntimeError)  { @h.each_pair {|k, v| @h.rehash } }
    assert_raise(RuntimeError)  { @h.each_value {|v| @h.rehash } }
    assert_raise(RuntimeError)  { @h.select {|k, v| @h.rehash } }

    h1 = {}
    # The iteration terminates on the first non-match, so we test that with
    # retval=false.
    h1[1] = RehashingObject.new(h1, false)
    h2 = {1 => 2}
    assert_raise(RuntimeError) { h1 == h2 }
    # Only the LHS has this check.
    # FIXME: If RHS is not a Hash but responds to to_hash, the order would be
    # reversed.
    assert_nothing_raised { h2 == h1 }
    # Does not affect comparing the same object.
    assert_nothing_raised { h1 == h1 }

    assert_raise(RuntimeError) { h1.index(RehashingObject.new(h1)) }
    assert_raise(RuntimeError) { h1.has_value?(RehashingObject.new(h1)) }
    assert_raise(RuntimeError) { h1.value?(RehashingObject.new(h1)) }

    # This does not use iteration, so don't raise exception.
    assert_nothing_raised { h1.values_at(RehashingObject.new(h1), RehashingObject.new(h1)) }
  end

  def test_reject_works_on_copy
    h = @h.reject { false }
    assert_not_same(h, @h)
  end

  def test_replace_replaces_default
    h = Hash.new('1234')
    @h.replace(h)
    assert_equal('1234', @h['11'])

    h = Hash.new {|h, k| k }
    @h.replace(h)
    assert_equal('5678', @h['5678'])
  end

  def test_replace_works_on_self_returns_self
    h = @h
    assert_same(h, @h.replace({ 'b'=>5, 'c'=>6 }))
    assert_same(h, @h)
  end

  def test_replace_arity
    assert_raise(ArgumentError) { @h.replace }
    assert_raise(ArgumentError) { @h.replace(@h, @h) }
  end

  def test_replace_nonhash
    assert_raise(TypeError) { @h.replace(['c', 3]) }
    assert_raise(TypeError) { @h.replace(StringToHash.new) }
    assert_equal({'z'=>'9'}, @h.replace(HashToHash.new))
  end

  def test_select_arity
    # 0
    assert_raise(ArgumentError) { @h.select('1') }
    assert_raise(ArgumentError) { @h.select('1') { true } }
  end

  def test_select_no_block
    assert_raise(LocalJumpError) { @h.select }
  end

  def test_select_block_returns_nonbool
    h = {'a'=>5 }
    assert_equal([], h.select { nil })
    assert_equal([['a', 5]], h.select { 0 })
  end

  def test_self_aref_with_hash
    a1 = [1, 2]
    a2 = [1, 2, 3]
    h1 = {a1=>a2}
    h2 = Hash[h1]
    assert_not_same(h1, h2)
    assert_same(h1.keys[0], h2.keys[0])
    assert_same(h1.values[0], h2.values[0])

    h3 = Hash[h1, a1]
    assert_same(h1, h3.keys[0])
    assert_same(a1, h3.values[0])
  end

  def test_self_aref_odd_arguments
    assert_raise(ArgumentError) { Hash[{}, 1, 2] }
  end

  def test_shift_empty_returns_default
    h = Hash.new(1)
    assert_equal(1, h.shift)
    h = Hash.new {|h, k| k.inspect }
    assert_equal('nil', h.shift)
  end

  def test_size_arity
    util_0_arity(:size)
  end

  def test_to_hash_returns_self
    assert_same(@h, @h.to_hash)
  end

  def test_to_s_recursive
    h = {}
    h[{h=>h}] = {h=>h}
    comma = $,
    $, = ':'
    s = h.to_s
    $, = comma
    assert_equal('{...}:{...}:{...}:{...}', s)
  end

  def test_to_s_arity
    # 0
    util_0_arity(:to_s)
  end

  # Utilities

  class AlwaysEqualKey
    def eql?(other)
      true
    end
    def hash
      0
    end
  end

  class AlwaysEqualValue
    def ==(other)
      true
    end
  end

  class BignumHash
    def hash
      20 ** 20
    end
  end

  class HashToHash
    def to_hash
      return {'z'=>'9'}
    end
  end

  class HasPercentHash
    class TruePercent
      def %(x)
        true # not Fixnum
      end
    end
    def hash
      TruePercent.new
    end
  end

  class NilHash
    def hash
      nil
    end
  end

  class NoPercentHash
    def hash
      Object.new # does not respond to %
    end
  end

  class RehashingObject
    def initialize(hash, retval=true)
      @hash = hash
      @retval = retval
    end
    def ==(other)
      @hash.rehash
      @retval
    end
  end

  class StringToHash
    def to_hash
      'z'
    end
  end

  def util_0_arity(meth)
    assert_raise(ArgumentError) { @h.send(meth, 0) }
  end

end
