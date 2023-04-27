// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Services;
using UnityEngine;
using RealityCollective.ServiceFramework.Interfaces;
using System.Text;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RealityCollective.ServiceFramework
{
    public static class ValidateConfiguration
    {
        private const string IgnoreKey = "_ServiceFramework_Editor_IgnorePrompts";

        /// <summary>
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="providerTypesToValidate">Array of Data Provider types to validate</param>
        /// <param name="providerDefaultConfiguration">Array of Data Provider default configurations to add if missing</param>
        /// <param name="prompt">Unit Test helper, to control whether the UI prompt is offered or not</param>
        /// <returns></returns>
        public static bool ValidateService<T>(this BaseServiceProfile<T> profile, Type[] providerTypesToValidate, IServiceConfiguration<T>[] providerDefaultConfiguration, bool prompt = true) where T : IService
        {
#if UNITY_EDITOR
            if (Application.isPlaying || EditorPrefs.GetBool(IgnoreKey, false))
            {
                return false;
            }
#endif //UNITY_EDITOR

            if (ServiceManager.IsActiveAndInitialized && ServiceManager.Instance.HasActiveProfile)
            {
                var errorsFound = false;

                if (profile == null)
                {
                    return false;
                }

                var registeredConfigurations = profile.ServiceConfigurations;

                if (providerTypesToValidate != null &&
                    providerTypesToValidate.Length > 0)
                {
                    var typesValidated = new bool[providerTypesToValidate.Length];

                    for (int i = 0; i < providerTypesToValidate.Length; i++)
                    {
                        if (providerTypesToValidate[i] == null) { continue; }

                        for (var j = 0; j < registeredConfigurations.Length; j++)
                        {
                            var subProfile = registeredConfigurations[j];

                            if (subProfile.InstancedType?.Type == providerTypesToValidate[i])
                            {
                                typesValidated[i] = true;
                            }
                        }
                    }

                    for (var i = 0; i < typesValidated.Length; i++)
                    {
                        if (!typesValidated[i])
                        {
                            errorsFound = true;
                        }
                    }

                    if (errorsFound)
                    {
                        var errorDescription = new StringBuilder();
                        errorDescription.AppendLine($"The following service modules were not found in the current {nameof(ServiceProvidersProfile)}:\n");

                        for (int i = 0; i < typesValidated.Length; i++)
                        {
                            if (!typesValidated[i])
                            {
                                errorDescription.AppendLine($" [{providerTypesToValidate[i]}]");
                            }
                        }

                        errorDescription.AppendLine($"\nYou can either add this manually in\nInput Profile ->  Controller service modules\n or click 'App Provider' to add this automatically");
#if UNITY_EDITOR
                        if (prompt)
                        {
                            if (EditorUtility.DisplayDialog($"{providerTypesToValidate[0]} provider not found", errorDescription.ToString(), "Ignore", "Add Provider"))
                            {
                                EditorPrefs.SetBool(IgnoreKey, true);
                            }
                            else
                            {
                                for (int i = 0; i < providerTypesToValidate.Length; i++)
                                {
                                    if (!typesValidated[i])
                                    {
                                        profile.AddConfiguration(providerDefaultConfiguration[i]);
                                    }
                                }

                                return true;
                            }
                        }
                        else
                        {
                            Debug.LogWarning(errorDescription);
                        }
#endif //UNITY_EDITOR
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}