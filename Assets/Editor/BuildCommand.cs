using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace GeoViewer.Editor
{
    internal static class BuildCommand
    {
        private const string KeystorePass = "KEYSTORE_PASS";
        private const string KeyAliasPass = "KEY_ALIAS_PASS";
        private const string KeyAliasName = "KEY_ALIAS_NAME";
        private const string Keystore = "keystore.keystore";
        private const string BuildOptionsEnvVar = "BuildOptions";
        private const string AndroidBundleVersionCode = "VERSION_BUILD_VAR";
        private const string AndroidAppBundle = "BUILD_APP_BUNDLE";
        private const string ScriptingBackendEnvVar = "SCRIPTING_BACKEND";
        private const string VersionNumberVar = "VERSION_NUMBER_VAR";
        private const string VersionIOS = "VERSION_BUILD_VAR";

        private static string GetArgument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].Contains(name))
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        private static string[] GetEnabledScenes()
        {
            return (
                from scene in EditorBuildSettings.scenes
                where scene.enabled
                where !string.IsNullOrEmpty(scene.path)
                select scene.path
            ).ToArray();
        }

        private static BuildTarget GetBuildTarget()
        {
            var buildTargetName = GetArgument("customBuildTarget");
            Console.WriteLine(":: Received customBuildTarget " + buildTargetName);

            if (string.Equals(buildTargetName, "android", StringComparison.OrdinalIgnoreCase))
            {
#if !UNITY_5_6_OR_NEWER
			// https://issuetracker.unity3d.com/issues/buildoptions-dot-acceptexternalmodificationstoplayer-causes-unityexception-unknown-project-type-0
			// Fixed in Unity 5.6.0
			// side effect to fix android build system:
			EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
#endif
            }

            if (buildTargetName.TryConvertToEnum(out BuildTarget target))
            {
                return target;
            }

            Console.WriteLine(
                $":: {nameof(buildTargetName)} \"{buildTargetName}\" not defined on enum {nameof(BuildTarget)}, using {nameof(BuildTarget.NoTarget)} enum to build");

            return BuildTarget.NoTarget;
        }

        private static string GetBuildPath()
        {
            var buildPath = GetArgument("customBuildPath");
            Console.WriteLine(":: Received customBuildPath " + buildPath);
            if (buildPath?.Length == 0)
            {
                throw new Exception("customBuildPath argument is missing");
            }

            return buildPath;
        }

        private static string GetBuildName()
        {
            var buildName = GetArgument("customBuildName");
            Console.WriteLine(":: Received customBuildName " + buildName);
            if (buildName?.Length == 0)
            {
                throw new Exception("customBuildName argument is missing");
            }

            return buildName;
        }

        private static string GetFixedBuildPath(BuildTarget buildTarget, string buildPath, string buildName)
        {
            if (buildTarget.ToString().Contains("windows", StringComparison.OrdinalIgnoreCase))
            {
                buildName += ".exe";
            }
            else if (buildTarget == BuildTarget.Android)
            {
#if UNITY_2018_3_OR_NEWER
                buildName += EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk";
#else
            buildName += ".apk";
#endif
            }

            return buildPath + buildName;
        }

        private static BuildOptions GetBuildOptions()
        {
            if (TryGetEnv(BuildOptionsEnvVar, out var envVar))
            {
                var allOptionVars = envVar.Split(',');
                var allOptions = BuildOptions.None;
                BuildOptions option;
                string optionVar;
                var length = allOptionVars.Length;

                Console.WriteLine($":: Detecting {BuildOptionsEnvVar} env var with {length} elements ({envVar})");

                for (var i = 0; i < length; i++)
                {
                    optionVar = allOptionVars[i];

                    if (optionVar.TryConvertToEnum(out option))
                    {
                        allOptions |= option;
                    }
                    else
                    {
                        Console.WriteLine($":: Cannot convert {optionVar} to {nameof(BuildOptions)} enum, skipping it.");
                    }
                }

                return allOptions;
            }

            return BuildOptions.None;
        }

        // https://stackoverflow.com/questions/1082532/how-to-tryparse-for-enum-value
        private static bool TryConvertToEnum<TEnum>(this string strEnumValue, out TEnum value)
        {
            if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
            {
                value = default;
                return false;
            }

            value = (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
            return true;
        }

        private static bool TryGetEnv(string key, out string value)
        {
            value = Environment.GetEnvironmentVariable(key);
            return !string.IsNullOrEmpty(value);
        }

        private static void SetScriptingBackendFromEnv(BuildTarget platform)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(platform);
            if (TryGetEnv(ScriptingBackendEnvVar, out var scriptingBackend))
            {
                if (scriptingBackend.TryConvertToEnum(out ScriptingImplementation backend))
                {
                    Console.WriteLine($":: Setting ScriptingBackend to {backend}");
                    PlayerSettings.SetScriptingBackend(targetGroup, backend);
                }
                else
                {
                    var possibleValues = string.Join(", ",
                        Enum.GetValues(typeof(ScriptingImplementation)).Cast<ScriptingImplementation>());
                    throw new Exception(
                        $"Could not find '{scriptingBackend}' in ScriptingImplementation enum. Possible values are: {possibleValues}");
                }
            }
            else
            {
                var defaultBackend = PlayerSettings.GetDefaultScriptingBackend(targetGroup);
                Console.WriteLine(
                    $":: Using project's configured ScriptingBackend (should be {defaultBackend} for targetGroup {targetGroup}");
            }
        }

        private static void PerformBuild()
        {
            var buildTarget = GetBuildTarget();

            Console.WriteLine(":: Performing build");
            if (TryGetEnv(VersionNumberVar, out var bundleVersionNumber))
            {
                if (buildTarget == BuildTarget.iOS)
                {
                    bundleVersionNumber = GetIosVersion();
                }

                Console.WriteLine(
                    $":: Setting bundleVersionNumber to '{bundleVersionNumber}' (Length: {bundleVersionNumber.Length})");
                PlayerSettings.bundleVersion = bundleVersionNumber;
            }

            if (buildTarget == BuildTarget.Android)
            {
                HandleAndroidAppBundle();
                HandleAndroidBundleVersionCode();
                HandleAndroidKeystore();
            }

            var buildPath = GetBuildPath();
            var buildName = GetBuildName();
            var buildOptions = GetBuildOptions();
            var fixedBuildPath = GetFixedBuildPath(buildTarget, buildPath, buildName);

            SetScriptingBackendFromEnv(buildTarget);

            var buildReport = BuildPipeline.BuildPlayer(GetEnabledScenes(), fixedBuildPath, buildTarget, buildOptions);

            if (buildReport.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new Exception($"Build ended with {buildReport.summary.result} status");
            }

            Console.WriteLine(":: Done with build");
        }

        private static void HandleAndroidAppBundle()
        {
            if (TryGetEnv(AndroidAppBundle, out var value))
            {
#if UNITY_2018_3_OR_NEWER
                if (bool.TryParse(value, out var buildAppBundle))
                {
                    EditorUserBuildSettings.buildAppBundle = buildAppBundle;
                    Console.WriteLine($":: {AndroidAppBundle} env var detected, set buildAppBundle to {value}.");
                }
                else
                {
                    Console.WriteLine(
                        $":: {AndroidAppBundle} env var detected but the value \"{value}\" is not a boolean.");
                }
#else
            Console.WriteLine($":: {ANDROID_APP_BUNDLE} env var detected but does not work with lower Unity version than 2018.3");
#endif
            }
        }

        private static void HandleAndroidBundleVersionCode()
        {
            if (TryGetEnv(AndroidBundleVersionCode, out var value))
            {
                if (int.TryParse(value, out var version))
                {
                    PlayerSettings.Android.bundleVersionCode = version;
                    Console.WriteLine(
                        $":: {AndroidBundleVersionCode} env var detected, set the bundle version code to {value}.");
                }
                else
                {
                    Console.WriteLine(
                        $":: {AndroidBundleVersionCode} env var detected but the version value \"{value}\" is not an integer.");
                }
            }
        }

        private static string GetIosVersion()
        {
            if (TryGetEnv(VersionIOS, out var value))
            {
                if (int.TryParse(value, out var version))
                {
                    Console.WriteLine($":: {VersionIOS} env var detected, set the version to {value}.");
                    return version.ToString();
                }

                Console.WriteLine(
                    $":: {VersionIOS} env var detected but the version value \"{value}\" is not an integer.");
            }

            throw new ArgumentNullException(nameof(value), $":: Error finding {VersionIOS} env var");
        }

        private static void HandleAndroidKeystore()
        {
#if UNITY_2019_1_OR_NEWER
            PlayerSettings.Android.useCustomKeystore = false;
#endif

            if (!File.Exists(Keystore))
            {
                Console.WriteLine($":: {Keystore} not found, skipping setup, using Unity's default keystore");
                return;
            }

            PlayerSettings.Android.keystoreName = Keystore;

            if (TryGetEnv(KeyAliasName, out var keyAliasName))
            {
                PlayerSettings.Android.keyaliasName = keyAliasName;
                Console.WriteLine($":: using ${KeyAliasName} env var on PlayerSettings");
            }
            else
            {
                Console.WriteLine($":: ${KeyAliasName} env var not set, using Project's PlayerSettings");
            }

            if (!TryGetEnv(KeystorePass, out var keystorePass))
            {
                Console.WriteLine($":: ${KeystorePass} env var not set, skipping setup, using Unity's default keystore");
                return;
            }

            if (!TryGetEnv(KeyAliasPass, out var keystoreAliasPass))
            {
                Console.WriteLine($":: ${KeyAliasPass} env var not set, skipping setup, using Unity's default keystore");
                return;
            }
#if UNITY_2019_1_OR_NEWER
            PlayerSettings.Android.useCustomKeystore = true;
#endif
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasPass = keystoreAliasPass;
        }
    }
}