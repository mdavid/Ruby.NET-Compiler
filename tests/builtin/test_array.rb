require 'test/unit'

class TestArray < Test::Unit::TestCase
  # TODO:
  # - rb_ary_at line 1
  # - rb_ary_splice line 3
  # - rb_ary_eql lines 3-5
  # - rb_ary_transpose line 5: special case, do we need to test this?
  # - rb_ary_replace line 5: special case, do we need to test this?
  # - What about warning-only C-blocks?

  def setup
    @arr = Array[1, 2, 3, Array[4, 5], 6]
  end

  # TODO: test other methods calling splice
  def test_aref_length_too_big
    assert_equal([6], @arr[4, 100])
  end

  def test_aref_negative_length
    assert_nil(@arr[1, -1])
  end

  def test_aref_range_out_of_range
    assert_nil(@arr[100..100])
  end

  def test_aref_symbol
    x = 1
    assert_raise(TypeError) { @arr[:x] }
    assert_raise(TypeError) { @arr[:x, 2] }
    assert_nothing_raised { @arr[2, :x] }
  end

  def test_aref_arity
    assert_raise(ArgumentError) { @arr[] }
    assert_raise(ArgumentError) { @arr[1, 2, 3] }
    assert_raise(ArgumentError) { @arr[1, 2, 3, 4] }
  end

  def test_aset_index_too_small
    assert_raise(IndexError) { @arr[-100] = 0 }
  end

  def test_aset_symbol
    x = 1
    assert_raise(TypeError) { @arr[:x] = 1 }
    assert_raise(TypeError) { @arr[1, :x] = 1 }
    assert_raise(TypeError) { @arr[:x, 2] = 1 }
  end

  def test_aset_arity
    assert_raise(ArgumentError) { @arr[] = 1 }
    assert_raise(ArgumentError) { @arr[1, 2, 3] = 1}
    assert_raise(ArgumentError) { @arr[1, 2, 3, 4] = 1}
  end

  def test_hash_with_nil_element
    assert_nothing_raised { [nil].hash }
    assert_nothing_raised { [1, nil, 2].hash }
  end

  def test_hash_with_custom_element_hash
    # nil
    assert_raise(TypeError) { [NilHash.new].hash }

    # In-range float
    assert_nothing_raised { [FloatHash.new].hash }
    # Too large float
    assert_raise(RangeError) { [BigFloatHash.new].hash }

    # Bignum
    assert_raise(RangeError) { [BignumHash.new].hash }

    # Other object (calls to_int)
    assert_raise(TypeError) { [StringHash.new].hash }
    assert_nothing_raised { [CustomHash.new].hash }
    assert_raise(TypeError) { [CustomHashWeirdToInt.new].hash }
  end

  def test_indexes
    # TODO: MRI uses aref; should we test everything again?
    old_w = $-w
    $-w = nil  # suppress Array#indexes warning
    assert_equal([2, 3], @arr.indexes(1, 2))
    $-w = old_w
  end

  def test_insert_end
    # Special case in MRI; do we need to test this?
    arr = Array[1, 2, 3]
    arr.insert(-1, 1,2,3)
    assert_equal([1, 2, 3, 1, 2, 3], arr)
  end

  def test_insert_arity
    assert_raise(ArgumentError) { @arr.insert }
  end

  def test_join_recursive
    # Requires <<
    arr = Array[1, 2]
    arr << arr
    assert_equal('1--2--1--2--[...]', arr.join('--'))
  end

  def test_select_arity
    assert_raise(ArgumentError) { @arr.select(0) }
    assert_raise(ArgumentError) { @arr.select(0, 1) }
  end

  def test_slice_bang_negative_length
    assert_nil(@arr[1, -1])
  end

  def test_sort_bang_frozen
    util_frozen_during_iteration(:sort!)
  end

  def test_to_a_subclass
    myarr = MyArray[1, 2, 3]
    arrmyarr = myarr.to_a
    assert_instance_of(Array, arrmyarr)
    assert_not_equal(myarr.__id__, arrmyarr.__id__) # maybe not needed
  end

  def test_zip_block
    # Requires <<

    arr1 = [1, 2]
    arr2 = [3, 4]
    arr3 = [5, 6]

    res = []
    assert_nil(arr1.zip {|a, b| res << a } )
    assert_equal([1, 2], res)
    assert_equal([1, 2], arr1)
    assert_not_same(res, arr1)

    res = []
    assert_nil(arr1.zip(arr2) {|a, b| res << b } )
    assert_equal([3, 4], res)
    assert_equal([1, 2], arr1)
    assert_equal([3, 4], arr2)

    res = []
    assert_nil(arr1.zip(arr2, arr3) {|a, b| res << b } )
    assert_equal([3, 4], res)
    assert_equal([1, 2], arr1)
    assert_equal([3, 4], arr2)
    assert_equal([5, 6], arr3)
  end

  # Utilities

  class MyArray < Array
  end

  # Used for testing Array#hash
  class NilHash; def hash; nil; end; end
  class FloatHash; def hash; 1.3; end; end
  class BigFloatHash; def hash; Float(20 ** 20); end; end
  class BignumHash; def hash; 20 ** 20; end; end
  class StringHash; def hash; 'a'; end; end
  class CustomHash; def hash; self; end; def to_int; 5; end; end
  class CustomHashWeirdToInt; def hash; self; end; def to_int; self; end; end

  def util_frozen_during_iteration(meth)
    # Only used by sort!, actually.

    frozen = true
    @arr.send(meth) {|a, b| if not @arr.frozen? then frozen = false end; 0 }
    assert(frozen)

    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr[0] = 0 } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr[0, 1] = 0 } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.collect! } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.compact! } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.concat([0]) } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.delete(1) } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.delete_at(0) } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.delete_if } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.fill(0) } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.flatten! } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.insert(0, 1) } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.pop } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.reject! } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.reverse! } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.shift } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.slice!(0, 1) } }
    assert_raise(RuntimeError) { @arr.send(meth) {|a, b| @arr.unshift(0) } }
  end
end
