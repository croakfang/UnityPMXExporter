![image](https://github.com/croakfang/UnityPMXExporter/assets/32562737/a9038ea3-cd05-42d7-912d-f311ea0e9a2a)

# Features

|||| 
| ------------ | ------------ | ------------ |
| ✓ - Working | / - Incomplete  | x - Unsupported  |

|||
| ------------ | ------------ |
| Vertices | ✓ |
| Triangles | ✓ |
| Material | / |
| Vertices Color | ✓ |
| UV 1-4 | ✓ |
| Texture | ✓ |
| Skeleton Rigging Skinning | ✓ |
| BlendShape | ✓ |
| RigidBody | x |
| Joint | x |

# Usage
- Import this plugin
- For Editor：
  - Right click on the object on the hierarchy
  - Exprot PMX Model ![image](https://github.com/croakfang/UnityPMXExporter/assets/32562737/cff4363b-3d32-4fb8-837c-81d6c1ab4ad2)

- For Runtime：
  ```
  using UnityPMXExporter;
  ...
  public GameObject Target;
  public string Path = "Assets/model.pmx"
  ...
  ModelExporter.ExportModel(Target, Path);
  ```

Special Thank to:
- hobosore: [UnityPMXRuntimeLoader](https://github.com/hobosore/UnityPMXRuntimeLoader) for defining the structure of the PMX model
- CherishingWish for exporting the texture
