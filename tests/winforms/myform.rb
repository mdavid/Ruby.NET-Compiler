# This Windows Forms example must be compiled first:
# rubycompiler myform.rb mscorlib.dll System.Windows.Forms.dll System.Drawing.dll

require 'mscorlib.dll'
require 'System.dll'
require 'System.Data.dll'
require 'System.Drawing.dll'
require 'System.Deployment.dll'
require 'System.Windows.Forms.dll'
require 'System.Xml.dll'

class Form1 < System::Windows::Forms::Form
	def initialize
		self.InitializeComponent
	end
	
	def InitializeComponent
		#puts "InitializeComponent"
		@textBox1 = System::Windows::Forms::TextBox.new
		@textBox2 = System::Windows::Forms::TextBox.new
		@textBox3 = System::Windows::Forms::TextBox.new
		@button1 = System::Windows::Forms::Button.new
		self.SuspendLayout
		
		@textBox1.set_Name("textBox1")
		@textBox1.set_Size(System::Drawing::Size.new(100, 20))		
		@textBox1.set_Location(System::Drawing::Point.new(24, 21))
		get_Controls.Add(@textBox1)
		
		@textBox2.set_Name("textBox2")
		@textBox2.set_Size(System::Drawing::Size.new(100, 20))
		@textBox2.set_Location(System::Drawing::Point.new(154, 21))
		get_Controls.Add(@textBox2)

		@textBox3.set_Name("textBox3")
		@textBox3.set_Size(System::Drawing::Size.new(100, 20))
		@textBox3.set_Location(System::Drawing::Point.new(300, 21))
		get_Controls.Add(@textBox3)

		@button1.set_Name("button1")
		@button1.set_Size(System::Drawing::Size.new(167, 32))
		@button1.set_Text("Add!")
		@button1.set_Location(System::Drawing::Point.new(167, 80))
		
		get_Controls.Add(@button1)

		set_ClientSize(System::Drawing::Size.new(600, 300))
		set_Name("Form1")
		set_Text("Form1")
		self.ResumeLayout(false)
		self.PerformLayout
	end
end

System::Windows::Forms::Application.EnableVisualStyles
System::Windows::Forms::Application.SetCompatibleTextRenderingDefault(false)
f = Form1.new
f.InitializeComponent
System::Windows::Forms::Application.Run(f)



