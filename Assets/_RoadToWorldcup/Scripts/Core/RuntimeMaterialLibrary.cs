using UnityEngine;

namespace RoadToWorldcup
{
    public static class RuntimeMaterialLibrary
    {
        private const string MaterialResourcePath = "RoadToWorldcupMobileMaterial";
        private static Material template;

        public static Material Create(string name, Color color)
        {
            if (template == null)
            {
                template = Resources.Load<Material>(MaterialResourcePath);
            }

            if (template == null)
            {
                throw new System.InvalidOperationException("The mobile runtime material is missing from Resources.");
            }

            Material material = new Material(template);
            material.name = name;
            material.color = color;
            return material;
        }
    }
}
