require 'System.dll'
require 'System.Drawing.dll'
require 'System.Windows.Forms.dll'

class Form1 < System::Windows::Forms::Form
	def initialize
		self.InitializeComponent
	end
	
	def InitializeComponent
		self.SuspendLayout
		
		self.ClientSize = System::Drawing::Size.new(300, 300)
		self.Name = "Form1"
		self.Text = "Form1"
		self.ResumeLayout(false)
		self.PerformLayout
	end
end