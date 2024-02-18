// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace RealityCollective.ServiceFramework.Definitions
{
    [System.Serializable]
    public class SceneServiceConfiguration
    {
        [SerializeField]
        private SceneServiceProvidersProfile profile;

        /// <inheritdoc />
        public SceneServiceProvidersProfile Profile
        {
            get => profile;
            internal set => profile = value;
        }
    }
}