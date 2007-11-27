CSC=gmcs -debug -warn:0

all: bin/Ruby bin/RubyCompiler.exe

clean:
	rm -f bin/Ruby
	rm -f bin/Ruby.NET.Runtime.dll bin/Ruby.exe bin/RubyCompiler.exe
	rm -f bin/*.mdb

test: bin/Ruby
	cd bin && ./Ruby test.rb

bin/Ruby.NET.Runtime.dll:
	$(CSC) -t:library -out:bin/Ruby.NET.Runtime.dll -r:Microsoft.Build.Utilities.dll -r:bin/QUT.ShiftReduceParser.dll -r:bin/QUT.PERWAPI.dll -recurse:src/RubyRuntime/*.cs

bin/Ruby.exe: bin/Ruby.NET.Runtime.dll
	$(CSC) -t:exe -out:bin/Ruby.exe -r:bin/Ruby.NET.Runtime.dll -recurse:src/Ruby/*.cs

bin/Ruby: bin/Ruby.exe
	cp bin/Ruby.sh bin/Ruby && chmod 755 bin/Ruby

bin/RubyCompiler.exe: bin/Ruby.NET.Runtime.dll
	$(CSC) -t:exe -out:bin/RubyCompiler.exe -r:bin/Ruby.NET.Runtime.dll -recurse:src/RubyCompiler/*.cs
