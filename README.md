
# ImageConvertResize.WPF (GPU)

Application WPF (.NET 8) de conversion + redimensionnement d’images avec :
- Aperçu avant/après, glisser-déposer
- Export rapide (PNG/JPG/WebP/TIFF/BMP/ICO)
- Traitement par lot avec parallélisme
- **Accélération GPU (DirectX 12)** via ComputeSharp — fallback WARP si pas de GPU

## Démarrer

```bash
dotnet restore
dotnet run
```

## Publier

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

## NuGet
- SixLabors.ImageSharp 3.1.x (décodage/encodage CPU)
- ComputeSharp 3.2.0 (DX12/WARP)

## Notes
- Le redimensionnement est effectué sur GPU (bicubique). Le padding carré pour l’ICO est aussi accéléré.
- Les ICO générés contiennent des PNG multi-résolutions (16–256 px).
