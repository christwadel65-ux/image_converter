# 🖼️ Image Converter Pro

Application WPF (.NET 8) moderne et professionnelle pour convertir et redimensionner vos images en lot avec facilité.

## 📁 Structure du Projet

```
image_converter/
├── 📂 src/
│   ├── 📂 Helpers/           # Classes utilitaires
│   │   ├── AvifHelper.cs     # Gestion du format AVIF (ImageMagick)
│   │   ├── GpuResizer.cs     # Redimensionnement GPU (ComputeSharp)
│   │   ├── IcoHelper.cs      # Création de fichiers ICO
│   │   └── ImageHelper.cs    # Utilitaires images (ImageSharp)
│   │
│   └── 📂 Windows/           # Fenêtres WPF
│       ├── MainWindow.xaml   # Fenêtre principale
│       ├── MainWindow.xaml.cs
│       ├── HelpWindow.xaml   # Fenêtre d'aide
│       └── HelpWindow.xaml.cs
│
├── 📂 docs/                  # Documentation
│   ├── README.md             # Documentation complète
│   ├── AMELIORATIONS.md      # Changelog des améliorations
│   └── LICENSE               # Licence MIT
│
├── 📂 bin/                   # Fichiers compilés
│   ├── Debug/
│   └── Release/
│       └── publish/          # Version standalone publiée
│
├── App.xaml                  # Point d'entrée WPF
├── App.xaml.cs
├── ImageConvertResize.WPF.csproj  # Fichier projet
└── image_converter.sln       # Solution Visual Studio
```

## ✨ Fonctionnalités Principales

### 📐 Conversion et Redimensionnement
- **Formats supportés** : JPG, PNG, WebP, **AVIF**, BMP, TIFF, ICO, GIF
- **Traitement par lot** : Convertissez plusieurs images simultanément
- **Redimensionnement intelligent** : Préserve les proportions automatiquement
- **Contrôle de qualité** : Réglez la qualité pour JPG, WebP et AVIF (0-100)
- **Métadonnées EXIF** : Préservation et rotation automatique selon EXIF

### 🚀 Performance
- **Traitement parallèle** : Utilise tous les cœurs CPU disponibles
- **Accélération GPU** : DirectX 12 via ComputeSharp (fallback WARP)
- **Format AVIF** : Compression optimale via ImageMagick

### 🎨 Interface Moderne
- **Aperçu en temps réel** : Visualisez avant/après avec statistiques
- **Glisser-déposer** : Interface intuitive et rapide
- **Préréglages de taille** : HD, Full HD, 4K, Instagram, Facebook, Miniature
- **Verrouillage du ratio** : Maintient les proportions automatiquement
- **Journal d'activité** : Suivi en temps réel du traitement

### 📊 Modes de Redimensionnement

| Mode | Description |
|------|-------------|
| **Ajuster (proportionnel)** | Conserve les proportions (défaut) |
| **Remplir (rogner)** | Remplit en rognant si nécessaire |
| **Étirer (déformer)** | Remplit exactement les dimensions |

## 🚀 Installation et Utilisation

### Option 1 : Version Standalone (Recommandée)
1. Téléchargez depuis `bin/Release/publish/`
2. Lancez `ImageConvertResize.exe`
3. Aucune installation nécessaire !

### Option 2 : Compilation depuis le code source
```powershell
# Clone le repository
git clone <votre-repo>
cd image_converter

# Compilation
dotnet build ImageConvertResize.WPF.csproj

# Ou pour créer une version standalone
dotnet publish ImageConvertResize.WPF.csproj -c Release
```

## 🛠️ Technologies Utilisées

- **.NET 8.0** : Framework moderne et performant
- **WPF (Windows Presentation Foundation)** : Interface utilisateur riche
- **SixLabors.ImageSharp 3.1.12** : Traitement d'images haute performance
- **ComputeSharp 3.2.0** : Accélération GPU via DirectX 12
- **Magick.NET 14.10.0** : Support AVIF et formats avancés

## 📋 Prérequis

- **Windows 10/11** (64-bit)
- **.NET 8.0 Runtime** (pour compilation uniquement)
- **GPU compatible DirectX 12** (optionnel, pour accélération GPU)

## 🎯 Guide d'Utilisation Rapide

1. **Chargez une image** : Cliquez sur "📂 Parcourir fichier" ou glissez-déposez
2. **Choisissez le format** : Sélectionnez JPG, PNG, WebP, AVIF, etc.
3. **Définissez les dimensions** : Utilisez les préréglages ou saisissez manuellement
4. **Réglez la qualité** : Pour JPG, WebP et AVIF (85 par défaut)
5. **Démarrez** : Cliquez sur "▶️ Démarrer le traitement"

### Traitement par Lot
1. Cliquez sur "📁 Parcourir dossier"
2. Sélectionnez le dossier contenant vos images
3. Activez "📂 Traiter sous-dossiers" si nécessaire
4. Choisissez le dossier de sortie
5. Lancez le traitement !

## 🌟 Format AVIF

Le format **AVIF** (AV1 Image File Format) est supporté via ImageMagick :
- ✅ **Meilleure compression** que JPEG et WebP
- ✅ **Qualité supérieure** à taille égale
- ✅ **Support de la transparence** comme PNG
- ✅ **Contrôle de qualité** de 0 à 100

## 📝 Licence

Projet sous licence **MIT** - Voir [LICENSE](docs/LICENSE)

## 📧 Support

Pour toute question ou suggestion, consultez la documentation complète dans [docs/README.md](docs/README.md) ou le changelog dans [docs/AMELIORATIONS.md](docs/AMELIORATIONS.md).

---

**Image Converter Pro v1.0.0**  
© 2025 - Auteur: C.L (Skill teams)
