# Complex build scenarios for Protobuf in C# 

My notes for dealing with more complex scenarios than noted in [GRPC Build Integration](https://github.com/grpc/grpc/blob/master/src/csharp/BUILD-INTEGRATION.md). I'm using [Grpc.Tools](https://www.nuget.org/packages/Grpc.Tools/) to build the proto files directly as part of the build process.

You can get around most issues by using custom build scripts and calling the proto compiler directly, but I wanted to push the "normal" tools and see if we could deal with the issues more elegantly. It's always easier if everything just works when you hit "Build."

There are a number of things that trip up the default compilation:

## Proto file location

Messages are defined outside the project tree, so we have to find them with `..\protos` and set the proto root correctly so the proto `import` directives work.

## Duplicate message names

Two messages called `Thing2` defined in separate `thing2.proto` files in different directories and namespaces.

This *should* be perfectly valid. They are different messages.

However, this normally results in Thing2.cs being generated twice and the second overwriting the first. Two ways to solve this:

- Rename the second `thing2.proto` to `thing2_2.proto` or something; just the proto file, not the contents -- this results in a different `.cs` file being generated, which solves the issue. Or just put all the messages in a single namespace file.
- Output compiled protos to specific directories so they don't conflict. 
    - The GRPC docs hint at using `%(RelativeDir)` to build to a specific location, but this is a slightly different use case. 
    - The most elegant way to deal with this is to build using `RecursiveDir` to place the output files in the `obj` build location like this: `OutputDir="$(IntermediateOutputPath)/%(RecursiveDir)"`. 
    - This also keeps Visual Studio happy as it is able to find the compiled files.

## Multiple packages and projects

We have two packages, with the second referencing the first. They are in different directories. Perhaps they're in different git repositories, managed by different teams. This causes issues with the default approach too.

There are a number of ways to tackle this:

- Copy everything into a single proto directory and use that the normal way (boring, didn't do this)
- Build everything in a single project [ProtoBuildAll](src/ProtoBuildAll) but refer to both sets of proto files; this is fine if you want a single library.
- Build separate libraries for `package1` and `package2`
    - This works well if you have all the protos readily available; refer to [ProtoBuild1](src/ProtoBuild1) and [ProtoBuild2](src/ProtoBuild2). 
        - The `package1` classes are not build twice in the second project, as `package1` is simply referenced as an include path. 
        - It all links up fine. I'd say this is the recommended approach.
    - If we *pretend we don't have the full schema* available for `package1` when building `package2`, we can cheat.
        - [ProtoBuild2Fake](src/ProtoBuild2Fake) is the example -- we refer to `proto-package1-fake` that has all the files and referenced by `package2`, but they are all just empty stubs. 
        - This also appears to work: they just need to be there to enable `package2` to compile, but the actual linked classes are from the real `ProtoBuild1` project.
        - Caveat emptor!

## Useful references

- [GRPC Build Integration](https://github.com/grpc/grpc/blob/master/src/csharp/BUILD-INTEGRATION.md)
- [Grpc.Tools source code](https://github.com/grpc/grpc/tree/master/src/csharp/Grpc.Tools)
- [MSBuild well-known item metadata](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-well-known-item-metadata?view=vs-2019)

