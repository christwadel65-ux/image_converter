# 🖼️ Image Converter Pro

Application WPF (.NET 8) moderne et professionnelle pour convertir et redimensionner vos images en lot avec facilité.

## ✨ Fonctionnalités Principales

### 📐 Conversion et Redimensionnement
- **Conversion de formats** : JPG, PNG, WebP, BMP, TIFF, ICO
- **Traitement par lot** : Convertissez plusieurs images à la fois
- **Redimensionnement intelligent** : Préserve les proportions ou ajustez comme vous le souhaitez
- **Contrôle de qualité** : Réglez la qualité pour JPG et WebP
- **Métadonnées EXIF** : Préservez les données de votre appareil photo

### 🚀 Performance
- **Traitement parallèle** : Utilise tous les cores de votre processeur
- **Accélération GPU (DirectX 12)** via ComputeSharp — fallback WARP si pas de GPU
- **Aperçu instantané** : Voir le résultat en temps réel

### 🎨 Interface Moderne et Intuitive
- **Aperçu avant/après** : Comparez les dimensions et la taille des fichiers
- **Glisser-déposer** : Déposez vos images directement dans l'application
- **Préréglages de taille** : HD, Full HD, 4K, Instagram, Facebook, Miniature
- **Verrouillage du ratio d'aspect** : Maintenez les proportions automatiquement
- **Journal d'activité** : Suivez la progression du traitement en temps réel

### 📊 Statistiques Détaillées
- Affichage automatique des dimensions de l'image originale
- Affichage de la taille du fichier
- Affichage des nouvelles dimensions après redimensionnement
- Calcul du pourcentage de réduction

## 🎯 Modes de Redimensionnement

| Mode | Description |
|------|-------------|
| **Ajuster (proportionnel)** | Redimensionne en conservant les proportions (par défaut) |
| **Remplir (rogner)** | Remplit les dimensions en rognant si nécessaire |
| **Étirer (déformer)** | Étire l'image pour remplir exactement les dimensions |

## 🚀 Installation et Utilisation

### Option 1 : Version Standalone (Recommandée)
1. Téléchargez les fichiers depuis le dossier `bin/Release/publish/`
2. Exécutez `ImageConvertResize.exe`
3. Aucune dépendance requise — l'application inclut tout ce dont elle a besoin

### Option 2 : Depuis le Code Source
```bash
# Clone le repository
git clone <votre-repo>
cd image_converter

# Restaure les dépendances
dotnet restore

# Lance l'application
dotnet run
```

## 📦 Publier une Version Standalone

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

Le fichier `ImageConvertResize.exe` sera généré dans `bin/Release/publish/`

## ⚙️ Configuration

### Dossier de Destination par Défaut
Par défaut, les images sont enregistrées dans :
```
C:\Mes Documents\image converter\
```

### Formats Supportés
- **Entrée** : JPG, JPEG, PNG, WebP, BMP, TIFF, TIF, GIF
- **Sortie** : JPG, PNG, WebP, BMP, TIFF, ICO

## 📋 Options Avancées

- ✅ Écraser les fichiers existants
- ✅ Conserver le nom d'origine (sans suffixe de dimensions)
- ✅ Traiter les sous-dossiers récursivement
- ✅ Préserver les métadonnées EXIF
- ✅ Rotation automatique selon EXIF
- ⚙️ Réglez la résolution (DPI)

## 🛠️ Technologies

- **Framework** : .NET 8 (.NET 8.0-windows)
- **UI** : WPF (Windows Presentation Foundation)
- **Traitement d'image** : SixLabors.ImageSharp 3.1.x
- **GPU** : ComputeSharp 3.2.0 (DirectX 12 / WARP fallback)
- **Architecture** : WinForms pour les dialogues de fichier

## 📦 Dépendances NuGet

```xml
<PackageReference Include="SixLabors.ImageSharp" Version="3.1.12" />
<PackageReference Include="SixLabors.ImageSharp.Drawing" Version="1.0.0" />
<PackageReference Include="ComputeSharp" Version="3.2.0" />
```

## 📝 Notes Importantes

- Le redimensionnement CPU utilise l'algorithme **Lanczos3** pour une meilleure qualité
- Les aperçus utilisent **Box resampler** pour une rapidité optimale
- Les ICO générés contiennent des PNG multi-résolutions (16–256 px)
- Support complet du **drag & drop** pour l'import d'images
- L'application crée automatiquement un dossier "image converter" dans le dossier de destination

## 🔧 Troubleshooting

**L'aperçu ne s'affiche pas ?**
- Assurez-vous que le fichier image est valide
- Essayez de récharger l'image avec le bouton "Fichier"

**Les images traitées ne sont pas sauvegardées ?**
- Vérifiez que le dossier de destination existe et est accessible
- Vérifiez les permissions d'accès au dossier

**Performance lente ?**
- Désactivez la "Préservation des métadonnées EXIF" pour les grands lots
- Utilisez une résolution (DPI) plus basse si elle n'est pas nécessaire

## 📄 Licence

Ce projet est fourni à titre d'exemple éducatif.

## 👨‍💻 Développement

Le code source est bien organisé et commenté :
- `MainWindow.xaml` - Interface utilisateur
- `MainWindow.xaml.cs` - Logique principale
- `ImageHelper.cs` - Utilitaires de traitement d'image
- `GpuResizer.cs` - Accélération GPU
- `IcoHelper.cs` - Génération de fichiers ICO
