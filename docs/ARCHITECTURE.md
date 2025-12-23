# 📐 Architecture du Projet Image Converter Pro

## 📂 Structure des Dossiers

```
image_converter/
│
├── 📄 App.xaml                           # Point d'entrée de l'application WPF
├── 📄 App.xaml.cs                        # Code-behind de l'application
├── 📄 ImageConvertResize.WPF.csproj      # Fichier projet .NET
├── 📄 image_converter.sln                # Solution Visual Studio
├── 📄 README.md                          # Documentation principale
│
├── 📂 src/                               # CODE SOURCE
│   │
│   ├── 📂 Helpers/                       # Classes utilitaires
│   │   ├── 🔧 AvifHelper.cs              # Gestion du format AVIF (ImageMagick)
│   │   │   └── LoadAvif()                # Charge un fichier AVIF
│   │   │   └── SaveAvif()                # Sauvegarde au format AVIF
│   │   │   └── IsAvif()                  # Vérifie si un fichier est AVIF
│   │   │
│   │   ├── 🚀 GpuResizer.cs              # Redimensionnement GPU (ComputeSharp)
│   │   │   └── Resize()                  # Redimensionne via GPU
│   │   │   └── ResizePadToPng()          # Redimensionne et centre via GPU
│   │   │   └── IsAvailable               # Vérifie la disponibilité du GPU
│   │   │
│   │   ├── 🎨 IcoHelper.cs               # Création de fichiers ICO
│   │   │   └── WriteIcoFromPngBlobs()    # Génère un ICO multi-résolution
│   │   │
│   │   └── 🖼️ ImageHelper.cs             # Utilitaires images généraux
│   │       └── GetInputFiles()           # Liste les fichiers images
│   │       └── ComputeTargetSize()       # Calcule dimensions cibles
│   │       └── AutoOrient()              # Rotation automatique EXIF
│   │       └── GetEncoder()              # Récupère l'encodeur approprié
│   │       └── LoadImage()               # Charge une image (tous formats)
│   │
│   └── 📂 Windows/                       # Fenêtres WPF
│       ├── 🪟 MainWindow.xaml            # Interface principale
│       ├── 📝 MainWindow.xaml.cs         # Logique de la fenêtre principale
│       │   └── StartButton_Click()       # Démarre le traitement par lot
│       │   └── ProcessFiles()            # Traite les fichiers en parallèle
│       │   └── LoadPreviewOriginal()     # Affiche l'aperçu original
│       │   └── GeneratePreviewProcessed()# Génère l'aperçu traité
│       │
│       ├── ℹ️ HelpWindow.xaml            # Fenêtre d'aide
│       └── 📝 HelpWindow.xaml.cs         # Logique de la fenêtre d'aide
│
├── 📂 docs/                              # DOCUMENTATION
│   ├── 📖 README.md                      # Guide utilisateur complet
│   ├── 📋 AMELIORATIONS.md               # Changelog et améliorations
│   └── 📜 LICENSE                        # Licence MIT
│
├── 📂 bin/                               # FICHIERS COMPILÉS
│   ├── Debug/                            # Version debug
│   └── Release/                          # Version release
│       └── publish/                      # 🎁 Version standalone publiée
│           └── ImageConvertResize.exe    # ⭐ Exécutable final
│
└── 📂 obj/                               # Fichiers temporaires de compilation
```

## 🔄 Flux de Traitement

```
1. Utilisateur charge une image
   └─> LoadPreviewOriginal()
        └─> ImageHelper.LoadImage()
             ├─> AVIF ? → AvifHelper.LoadAvif()
             └─> Autres formats → ImageSharp.Load()

2. Aperçu en temps réel
   └─> GeneratePreviewProcessed()
        └─> ImageHelper.ComputeTargetSize()
        └─> GPU disponible ?
             ├─> OUI → GpuResizer.Resize()
             └─> NON → CpuResize()

3. Traitement par lot
   └─> StartButton_Click()
        └─> ProcessFiles() [Parallèle]
             ├─> Pour chaque fichier:
             │    ├─> ImageHelper.LoadImage()
             │    ├─> ImageHelper.AutoOrient()
             │    ├─> GpuResizer.Resize() ou CpuResize()
             │    └─> Sauvegarde:
             │         ├─> ICO ? → IcoHelper.WriteIcoFromPngBlobs()
             │         ├─> AVIF ? → AvifHelper.SaveAvif()
             │         └─> Autres → ImageSharp.Save()
             │
             └─> Progression en temps réel
```

## 🛠️ Technologies par Composant

| Composant | Technologie | Version | Rôle |
|-----------|-------------|---------|------|
| **MainWindow** | WPF | .NET 8 | Interface utilisateur |
| **ImageHelper** | ImageSharp | 3.1.12 | Traitement d'images standard |
| **GpuResizer** | ComputeSharp | 3.2.0 | Accélération GPU (DirectX 12) |
| **AvifHelper** | Magick.NET | 14.10.0 | Support format AVIF |
| **IcoHelper** | Natif C# | - | Génération fichiers ICO |

## 📦 Dépendances NuGet

```xml
<PackageReference Include="SixLabors.ImageSharp" Version="3.1.12" />
<PackageReference Include="SixLabors.ImageSharp.Drawing" Version="1.0.0" />
<PackageReference Include="ComputeSharp" Version="3.2.0" />
<PackageReference Include="Magick.NET-Q8-AnyCPU" Version="14.10.0" />
```

## 🎯 Points d'Entrée

### Application
- **`App.xaml`** : Définit les ressources globales et le point d'entrée
- **`App.xaml.cs`** : Configuration de démarrage de l'application

### Fenêtre Principale
- **`MainWindow.xaml`** : Layout et contrôles de l'interface
- **`MainWindow.xaml.cs`** : Logique métier et traitement

### Helpers (Classes Utilitaires)
- **`ImageHelper.cs`** : Hub central pour les opérations images
- **`GpuResizer.cs`** : Redimensionnement accéléré par GPU
- **`AvifHelper.cs`** : Pont entre ImageSharp et ImageMagick pour AVIF
- **`IcoHelper.cs`** : Génération d'icônes Windows multi-résolution

## 🔐 Avantages de cette Architecture

✅ **Séparation des préoccupations** : UI séparée de la logique métier
✅ **Maintenabilité** : Code organisé par responsabilité
✅ **Réutilisabilité** : Helpers indépendants et testables
✅ **Évolutivité** : Facile d'ajouter de nouveaux formats ou fonctionnalités
✅ **Performance** : Traitement parallèle avec accélération GPU optionnelle
✅ **Documentation** : Structure claire et auto-documentée

## 📊 Métriques du Projet

- **Langages** : C# (.NET 8), XAML
- **Lignes de code** : ~800 lignes
- **Classes** : 6 classes principales
- **Fenêtres** : 2 fenêtres WPF
- **Formats supportés** : 8 formats d'entrée/sortie
- **Performances** : Traitement parallèle multi-thread + GPU

---

**Version** : 1.0.0  
**Dernière mise à jour** : 23 décembre 2025  
**Auteur** : C.L (Skill teams)
