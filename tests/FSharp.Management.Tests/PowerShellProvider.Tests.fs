module FSharp.Management.Tests.PowerShellProvider

open FSharp.Management
open Expecto
open System.Management.Automation
open System.Collections.ObjectModel

let [<Literal>]Modules = "Microsoft.PowerShell.Management;Microsoft.PowerShell.Core"

type PS = PowerShellProvider< Modules >

let [<Tests>] psTest =
    testList "PoweShell TP Tests" [
        test "Get system drive" {
            match PS.``Get-Item``(path=[|@"C:\"|]) with
            | Success(Choice6Of6 dirs) ->
                Expect.equal dirs.Length 1 ""
                Expect.equal ((Seq.head dirs).FullName) @"C:\" ""
            | _ -> failwith "Unexpected result"
        }
        test "Check that process `testtest` does not started" {
            match PS.``Get-Process``(name=[|"testtest"|]) with
            | Failure(_) -> ignore()
            | _ -> failwith "Unexpected result"
        }
        test "Get list of registered snapins" {
            match PS.``Get-PSSnapin``(registered = true) with
            | Success(Choice2Of2 (snapins)) ->
                Expect.equal (snapins.IsEmpty) false ""
            | _ -> failwith "Unexpected result"
        }
        test "Get random number from range" {
            match PS.``Get-Random``(minimum = 0, maximum = 10) with
            | Success(Choice2Of4 [value]) when value >=0 && value <= 10 -> ()
            | _ -> failwith "Unexpected result"
        }
        test "Get events from event log" {
            match PS.``Get-EventLog``(logName="Application", entryType=[|"Error"|], newest=2) with
            | Success(Choice3Of4 entries) ->
                Expect.equal entries.Length 2 ""
            | _ -> failwith "Unexpected result"
        }
        test "Get help message" { // This Cmdlet has an empty OutputType
            match PS.``Get-Help``() with
            | Success(resultObj) ->
                Expect.equal (resultObj.GetType()) (typeof<System.Management.Automation.PSObject>) ""
            | _ -> failwith "Unexpected result"
        }
        test "Get help message with custom runspace" { // Verify CustomRunspace existance and disposing
            let psCustom = new PS.CustomRunspace()

            match psCustom.``Get-Help``() with
            | Success(resultObj) ->
                Expect.equal (resultObj.GetType()) (typeof<System.Management.Automation.PSObject>) ""
            | _ -> failwith "Unexpected result"

            Expect.equal psCustom.Runspace.RunspaceStateInfo.State
                         System.Management.Automation.Runspaces.RunspaceState.Opened ""
            (psCustom :> System.IDisposable).Dispose()
            Expect.equal psCustom.Runspace.RunspaceStateInfo.State
                         System.Management.Automation.Runspaces.RunspaceState.Closed ""
        }
        test "Get a missing variable" { //Produces an exception
            match PS.``Get-Variable`` (name=[|"Missing"|], errorAction=ActionPreference.Stop) with
            | Failure (resultObj) ->
                let result = List.head resultObj
                Expect.equal (result.GetType()) (typeof<System.Management.Automation.ErrorRecord>) "" 
            | _ -> failwith "Unexpected result"
        }
        test "Get execution policies as a list" { //Produces a generic output
            match PS.``Get-ExecutionPolicy``(list=true) with
            | Success (Choice1Of2 result) -> 
                Expect.equal (result.Head.GetType()) (typeof<Collection<PSObject>>) ""
            | _ -> failwith "Unexpected result"
        }
    ]


let [<Literal>]ModuleFile = __SOURCE_DIRECTORY__ + @"\testModule.psm1"
type PSFileModule =  PowerShellProvider< ModuleFile >

let [<Tests>] psModuleTests =
    testCase "Call a function defined in a module file" <| fun _ ->
        let testString = "testString"
        match PSFileModule.doSomething(test=testString) with
        | Success(Choice2Of2 stringList) ->
            Expect.equal stringList.Length 1 ""
            Expect.equal (Seq.head stringList) testString ""
        | _ -> failwith "Unexpected result"

type PS64 = PowerShellProvider< Modules, Is64BitRequired=true >

let [<Tests>] ps64tests =
    testList "PowerShell Type Provider 64 Tests" [
        test "Get system drive x64" {
            match PS64.``Get-Item``(path=[|@"C:\"|]) with
            | Success(Choice6Of6 dirs) ->
                Expect.equal dirs.Length 1 ""
                Expect.equal ((Seq.head dirs).FullName) @"C:\" ""
            | _ -> failwith "Unexpected result"
        }
        test "Check that process `testtest` does not started x64" {
            match PS64.``Get-Process``(name=[|"testtest"|]) with
            | Failure(_) -> ignore()
            | _ -> failwith "Unexpected result"
        }
        test "Get list of registered snapins x64" {
            match PS64.``Get-PSSnapin``(registered = true) with
            | Success(Choice2Of2 snapins) ->
                Expect.equal snapins.IsEmpty false ""
            | _ -> failwith "Unexpected result"
        }
    ]

//let modules = "ActiveDirectory;AppBackgroundTask;AppLocker;Appx;AssignedAccess;Azure;BestPractices;BitLocker;BranchCache;CimCmdlets;ClusterAwareUpdating;DFSN;DFSR;Defender;DhcpServer;DirectAccessClientComponents;Dism;DnsClient;DnsServer;FailoverClusters;GroupPolicy;Hyper-V;ISE;International;IpamServer;IscsiTarget;Kds;MMAgent;Microsoft.PowerShell.Core;Microsoft.PowerShell.Diagnostics;Microsoft.PowerShell.Host;Microsoft.PowerShell.Management;Microsoft.PowerShell.Security;Microsoft.PowerShell.Utility;Microsoft.WSMan.Management;MsDtc;NFS;NetAdapter;NetConnection;NetEventPacketCapture;NetLbfo;NetNat;NetQos;NetSecurity;NetSwitchTeam;NetTCPIP;NetWNV;NetworkConnectivityStatus;NetworkLoadBalancingClusters;NetworkTransition;PKI;PSDesiredStateConfiguration;PSDiagnostics;PSScheduledJob;PSWorkflow;PcsvDevice;PrintManagement;RemoteAccess;RemoteDesktop;ScheduledTasks;SecureBoot;ServerManager;ServerManagerTasks;SmbShare;SmbWitness;StartScreen;Storage;TLS;TroubleshootingPack;TrustedPlatformModule;UpdateServices;VpnClient;Wdac;WebAdministration;WindowsDeveloperLicense;WindowsErrorReporting;WindowsSearch;iSCSI".Split([|';'|])