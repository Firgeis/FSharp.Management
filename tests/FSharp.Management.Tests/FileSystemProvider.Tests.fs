module FSharp.Management.Tests.FileSystemProviderTests

open FSharp.Management
open Expecto

type Users = FileSystem<"C:\\Users">
type RelativeUsers = FileSystem<"C:\\Users", "C:\\Users">

type Relative = RelativePath<"">
type RelativeToBin = RelativePath<"bin">
type RelativeToBuild = RelativePath<"bin\\Release">

let [<Tests>] fileSystemTests =
    testList "FileSystemProvider Tests" [
        test "Can create type for users path" {
            Expect.equal Users.Path @"C:\Users\" ""
        }
        test "Can access the default users path" {
            Expect.equal Users.Default.Path @"C:\Users\Default\" ""
        }
        test "Can access the users path via relative path" {
            Expect.equal RelativeUsers.Path "." ""
        }
        test "Can access the default users path via relative path" {
            Expect.equal RelativeUsers.Default.Path @"Default\" ""
        }
        test "Can access the bin folder within the project" {
            Expect.equal Relative.bin.Path @"bin\" ""
        }
        test "Can access the bin folder via \".\" in RelativePath provider" {
            Expect.equal RelativePath<".">.bin.Path @"bin\" ""
        }
        test "Can access a relative path" {
            Expect.equal Relative.Path @"." ""
        }
        test "Can access a relative subfolder" {
            Expect.equal Relative.bin.Path @"bin\" ""
        }
        test "Can access a relative subfolder relative to .\\bin" {
            Expect.equal RelativeToBin.Release.Path @"Release\" ""
        }
        test "Can access a relative file" {
            Expect.equal Relative.``WMI.Tests.fs`` @"WMI.Tests.fs" ""
        }
        test "Can access a parent dir" {
            Expect.equal Relative.``..``.Path @"..\" ""
        }
        test "Can access a parent's parent dir" {
            Expect.equal Relative.``..``.``..``.Path @"..\..\" ""
        }
        test "Can access solution files using RelativePath provider" {
            let fsDocPath = RelativeToBuild.``..``.``..``.``..``.``..``.docs.content.``FileSystemProvider.fsx``
            let buildFolder = CommonFolders.GetApplication ApplicationPath.FSharpManagementLocation

            let path = System.IO.Path.GetFullPath(System.IO.Path.Combine(buildFolder, fsDocPath))

            Expect.equal (System.IO.File.Exists(path)) true ""
        }
    ]