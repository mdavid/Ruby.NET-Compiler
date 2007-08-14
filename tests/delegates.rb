require "mscorlib"

handler = System::EventHandler.new { |sender,args| puts "EventHandler" }
handler.Invoke("sender", System::EventArgs.new)

run = System::Threading::ThreadStart.new { puts "ThreadStart" }
thread = System::Threading::Thread.new(run)
thread.Start
