// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;

public class SwarmClient : ModuleRules
{
	public SwarmClient(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[] 
        { 
            "Core", 
            "CoreUObject", 
            "Engine", 
            "InputCore", 
            "EnhancedInput",
            "NavigationSystem",
            "Navmesh",
            "Sockets",
            "Networking"
        });

        PrivateDependencyModuleNames.AddRange(new string[] 
        { 
            "Protobuf",
            "Json",
            "JsonUtilities",
        });

        PrivateIncludePaths.AddRange(new string[]
        {
            "SwarmClient/",
            "SwarmClient/Config/",
        });
    }
}
