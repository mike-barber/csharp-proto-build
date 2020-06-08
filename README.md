# Complex build scenarios for Protobuf in C# 

My notes for dealing with more complex scenarios than noted in (GRPC Build Integration)[https://github.com/grpc/grpc/blob/master/src/csharp/BUILD-INTEGRATION.md]. I'm using Grpc.Tools to build the proto files.

There are a number of things that trip up the default compilation:

- Messages are defined outside the project tree, so we have to find them with `..\protos` and set the proto root correctly so the proto `import` directives work.
- Two messages called `Thing2` defined in separate `thing2.proto` files in different directories and namespaces; this normally results in Thing2.cs being generated twice and the second overwriting the first. Two ways to solve this:
    - rename the second `thing2.proto` to `thing2_2.proto` or something; just the proto file, not the contents -- this results in a different .cs file being generated, which solves the issue. Or just put all the messages in a single namespace file.
    - output compiled protos to specific directories so they don't conflict. The GRPC docs hint at using `%(RelativeDir)` to build to a specific location, but this is a slightly different use case. The most elegant way to deal with this is to build using `RecursiveDir` to place the output files in the `obj` build location like this: `OutputDir="$(IntermediateOutputPath)/%(RecursiveDir)"`. This also keeps Visual Studio happy as it is able to find the compiled files.
- Two packages, with the second referencing the first. They are in different directories. There are a number of ways to solve this problem:
    - Copy everything into a single proto directory and use that the normal way (boring, didn't do this)
    - Build everything in a single project (ProtoBuildAll)[src/ProtoBuildAll] but refer to both sets of proto files; this is fine if you want a single library.
    - Build separate libraries for `package1` and `package2`
        - This works well if you have all the protos readily available; refer to (ProtoBuild1)[src/ProtoBuild1] and (ProtoBuild2)[src/ProtoBuild2]. The `package1` classes are not build twice in the second project, as `package1` is simply referenced as an include path. It all links up fine.
        - If we pretend we don't have the full schema available for `package1` when building `package2`, we can cheat. (ProtoBuild2Fake)[src/ProtoBuild2Fake] is the example -- we refer to `proto-package1-fake` that has all the files and referenced by `package2`, but they are all just empty stubs. This also appears to work: they just need to be there to enable `package2` to compile, but the actual linked classes are from the real `ProtoBuild1` project.


