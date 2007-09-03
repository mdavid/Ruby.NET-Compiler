require "mscorlib"
a = System::Collections::ArrayList.new

class System::Collections::ICollection
  def first
    self[0] if (self.Count > 0)
  end
  
  def last
    self[self.Count - 1] if (self.Count > 0)
  end
end

puts a.first
puts a.last

a.Add(0)
a.Add(1)

puts a.first
puts a.last
