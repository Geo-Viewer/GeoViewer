using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using GeoViewer.View.Rendering;
using UnityEngine;

namespace GeoViewer.Controller.DataLayers
{
    /// <summary>
    /// A class bundling all request Tasks for a single tile
    /// </summary>
    public class TileRequest
    {
        public TileId TileId { get; }
        private Task<Texture2D> TextureRequest { get; }
        public Task? TextureRender { get; private set; }
        private Task<IReadOnlyList<GlobePoint>> MeshRequest { get; }
        public Task? MeshRender { get; private set; }
        private CancellationTokenSource TokenSource { get; }

        public TileRequest((TileId, GlobeArea) request, ITextureLayer targetTextureLayer, IMeshLayer targetMeshLayer)
        {
            TokenSource = new CancellationTokenSource();
            TileId = request.Item1;
            TextureRequest = targetTextureLayer.RequestData(request, TokenSource.Token);
            MeshRequest = targetMeshLayer.RequestData(request, TokenSource.Token);
        }

        public TileRequest((TileId, GlobeArea) request, ITextureLayer targetTextureLayer, IMeshLayer targetMeshLayer,
            TileGameObject tileGameObject, MapRenderer mapRenderer)
            : this(request, targetTextureLayer, targetMeshLayer)
        {
            SetRenderTasks(targetTextureLayer, targetMeshLayer, tileGameObject, mapRenderer);
        }

        public void SetRenderTasks(ITextureLayer targetTextureLayer, IMeshLayer targetMeshLayer,
            TileGameObject tileGameObject, MapRenderer mapRenderer)
        {
            TextureRender = GetRenderTask(TextureRequest, targetTextureLayer);
            MeshRender = GetRenderTask(MeshRequest, targetMeshLayer);

            async Task GetRenderTask<TData>(Task<TData> requestTask, IDataRequest<TData> dataRequest)
            {
                dataRequest.RenderData(await requestTask, tileGameObject, mapRenderer);
            }
        }

        public void Cancel()
        {
            TokenSource.Cancel();
        }
    }
}