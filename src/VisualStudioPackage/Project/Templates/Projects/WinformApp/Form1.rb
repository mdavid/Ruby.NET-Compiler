require 'mscorlib.dll'
require 'System.dll'
require 'System.Drawing.dll'
require 'System.Windows.Forms.dll'


class Form1 < System::Windows::Forms::Form

  def initialize
	self.InitializeComponent
  end
	
  def InitializeComponent
      self.Text = 'Form1'
  end
  
end
