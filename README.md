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

### Some vertex information will be recorded in the additional UVs of PMX
| Unity | PMX |
| ----- | ----- |
| UV2 | Extra UV1 |
| UV3 | Extra UV2 |
| Vertex Color | Extra UV3 |

# Usage
- Import this plugin
- For Editor：
  - Right click on the object on the hierarchy
  - Export PMX Model
  - ![88c45375d85fe4a2e4c61c88500efa7c](https://github.com/user-attachments/assets/9ded1f65-a4ff-4bd4-a9dc-928145b40a99)


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
