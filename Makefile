CSC=gmcs -debug -warn:0

all: bin/Ruby.exe

clean:
	rm -f bin/Ruby.NET.Runtime.dll bin/Ruby.exe bin/*.mdb

bin/Ruby.NET.Runtime.dll:
	$(CSC) -t:library -out:bin/Ruby.NET.Runtime.dll -r:Microsoft.Build.Utilities.dll -r:bin/QUT.ShiftReduceParser.dll -r:bin/QUT.PERWAPI.dll -recurse:src/RubyRuntime/*.cs

bin/Ruby.exe: bin/Ruby.NET.Runtime.dll
	$(CSC) -t:exe -out:bin/Ruby.exe -r:bin/Ruby.NET.Runtime.dll -recurse:src/Ruby/*.cs
