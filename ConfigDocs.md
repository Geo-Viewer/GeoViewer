# Config Documentation
The config file is a json file found at

**Windows:** `AppData/Roaming/GeoViewer`

**Linux:** `$XDG_CONFIG_HOME/GeoViewer`, usually `~/.config/GeoViewer`

It will be reloaded every time the application starts.
---
## General Settings

| Name                 | Description                                                                                                                                       | Accepted Values                                 |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- |
| ConfigVersion        | The version of this config. **This should not be changed**                                                                                        | Any positive int                                |
| ResolutionMultiplier | Global multiplier for the map resolution. This determines tiles of which size should be loaded                                                    | Any value > 0                                   |
| MapSizeMultiplier    | A multiplier for the displayed map size                                                                                                           | Any value > 0                                   |
| MinMapSize           | The minimal radius of the displayed map in metres                                                                                                 | Any value > 0                                   |
| EnableTileCulling    | Whether the tiles outside the culling angle should be loaded at a lower resolution. This improves loading speed, especially for high resolutions. | true/false                                      |
| CullingAngle         | At which angle tiles should be culled. Higher value means less culling                                                                            | value from 0 to 180                             |
| DataLayers           | A list of all data layers used by GeoViewer                                                                                                       | List of [DataLayerSettings](#datalayersettings) |

## Graphic Settings

| Name | Description | Accepted Values |
|--------|-------------|------------------|
| CameraFov | Camera Field of View | 0 - 180 degrees |
| EnableDistanceFog | whether there should be rendered fog at map edges to hide loading of new tiles | true/false |
| EnablePostProcessing | Enables Color Correction at some performance cost | true/false |
| EnableVSync | Whether VSync should be used | true/false |
| TargetFrameRate | the framerate the application should target | any value > 0 |

## DataLayerSettings


### General Layer Settings

| Name              | Description                                                                                      | Accepted Values                                                                                                                                                        |
| ----------------- | ------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Type              | The type of the layer. This determines other available settings                                  | [BaseTexture](#base-texture-layer), [BaseMesh](#base-mesh-layer), [OsmTexture](#osm-texture-layer), [OtdMesh](#otd-mesh-layer), [TopoSharpMesh](#topoSharp-mesh-layer) |
| Name              | The name of this layer                                                                           | Any valid string                                                                                                                                                       |
| Priority          | The priority of this layer. The highest active layer will be rendered (for every rendering type) | Any int > 0                                                                                                                                                            |
| ParallelRequests  | The amount of requests to be send in parallel                                                    | Any int > 0                                                                                                                                                            |
| RequestsPerSecond | The amount of requests to be send per second                                                     | Any int >= 0, negative values are unlimited                                                                                                                            |
| CacheSize         | The amount of data entries to be cached                                                          | Any int >= 0                                                                                                                                                           |

### Layer Render Type Settings

#### Texture Layer

| Name                 | Description                                         | Accepted Values                                |
| -------------------- | --------------------------------------------------- | ---------------------------------------------- |
| FilterMode           | The filter mode to be used for this layers textures | 0 - Point, 1 - Bilinear, 2 - Trilinear         |
| SegmentationSettings | The segmentation settings for this layer            | [SegmentationSettings](#segmentation-settings) |

##### Segmentation Settings

| Name                 | Description                                            | Accepted Values           |
| -------------------- | ------------------------------------------------------ | ------------------------- |
| Projection           | The projection used by textures                        | WebMercator               |
| ZoomBounds           | Zoom bounds of the layer                               | integer [Bounds](#bounds) |
| ResolutionMultiplier | Multiplier for map resolution, specific for this layer | Any value > 0             |

##### Bounds

| Name          | Description | Accepted Values |
| ------------- | ----------- | --------------- |
| Min | minimal value | any value |
| Max | maximum value | any value >= Min |

#### Mesh Layer

| Name          | Description | Accepted Values |
| ------------- | ----------- | --------------- |
| MeshResolution | The amount of vertices at the edge of each tile | any int > 1 |

### Specific Layer Settings

#### Base Texture Layer

**Render Type:** Texture Layer

| Name          | Description | Accepted Values |
| ------------- | ----------- | --------------- |
| TexturePath | File path to an image file | any valid file path as a string |

#### Osm Texture Layer

**Render Type:** Texture Layer

| Name | Description             | Accepted Values                           |
| ---- | ----------------------- | ----------------------------------------- |
| Url  | Url to the tile service | A url containing {zoom}, {x} and {y} tags |

#### Bing Texture Layer

**Render Type:** Texture Layer

| Name | Description             | Accepted Values                           |
| ---- | ----------------------- | ----------------------------------------- |
| Url  | Url to the tile service | A url containing a {quadkey} tag |

####  Base-Mesh-Layer

**Render Type:** Mesh Layer

| Name          | Description | Accepted Values |
| ------------- | ----------- | --------------- |

#### Otd Mesh Layer

**Render Type:** Mesh Layer

| Name          | Description | Accepted Values |
| ------------- | ----------- | --------------- |
| Url | Url to a Opontopodata compatible service | any valid url |
| Interpolation | Interpolation used for height values | 0 - Nearest, 1 - Biliniear, 2 - Cubic |

#### TopoSharp Mesh Layer

**Render Type:** Mesh Layer

| Name          | Description | Accepted Values |
| ------------- | ----------- | --------------- |
| Url | Url to a TopoSharp (v2) compatible service | valid url with {minlat}, {maxlat}, {minlon}, {maxlon} and {resolution} tags |
