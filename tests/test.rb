$testnum = 0

def test_ok(cond)
  $testnum += 1
  if cond
    printf("ok %d\n", $testnum)
  else
    printf("not ok %d\n", $testnum)
  end
end

require 'complex'
number = Complex(1, 1)
test_ok(number * number.conjugate == number.abs2)

require 'csv'
line = '1,2,3'
test_ok(CSV.parse_line(line).join(',') == line)

require 'rational'
test_ok(1.quo(2) * 2 == 1)

require 'uri'
uri = 'http://example.com/'
test_ok(URI.parse(uri).to_s == uri)

require 'cgi'
string = 'Hello, world!'
test_ok(CGI.unescape(CGI.escape(string)) == string)

require 'date'
string = '1985-12-23'
test_ok(Date.parse(string).to_s == string)
