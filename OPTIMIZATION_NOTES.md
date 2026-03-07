# Menu Loading Optimization

## Problem
The mod was causing significant frame hitches when characters spawned because all menu UI elements were being instantiated immediately in `Awake()` methods. This included:

- All monster buttons (~50-100+ buttons)
- All equipment buttons (~100+ buttons)
- All item buttons (~200+ buttons)
- All buff buttons (~100+ buttons)
- All player stat fields (~50+ fields)

This resulted in hundreds of GameObjects being instantiated synchronously, causing a noticeable freeze.

## Solution: Lazy Loading with Coroutines

### Changes Made

1. **Deferred Initialization**: Moved button/field creation from `Awake()` to `OnEnable()` with lazy initialization
2. **Batched Creation**: Used coroutines to spread instantiation across multiple frames
3. **Increased Spawn Delay**: Changed UI spawn delay from 3 to 5 seconds in `Load.cs`

### Modified Files

- `Aerolt/Buttons/MonsterButtonGenerator.cs` - 10 buttons per frame
- `Aerolt/Buttons/EquipmentButtonGenerator.cs` - 15 buttons per frame
- `Aerolt/Buttons/EditPlayerItemButton.cs` - 15 buttons per frame
- `Aerolt/Buttons/EditMonsterItemButton.cs` - 15 buttons per frame
- `Aerolt/Buttons/EditPlayerBuffButton.cs` - 20 buttons per frame
- `Aerolt/Buttons/PlayerValuesGenerator.cs` - 10 fields per frame
- `Aerolt/Load.cs` - Increased spawn delay to 5 seconds

### How It Works

Each generator now:
1. Sets up lightweight components (dropdowns, listeners) in `Awake()`
2. Waits until the panel is first opened (`OnEnable()`)
3. Creates UI elements in batches using coroutines
4. Yields control back to Unity after each batch to prevent frame drops

### Performance Impact

- **Before**: All UI elements created at once = 1-2 second freeze
- **After**: UI elements created on-demand over multiple frames = smooth experience

### Tuning

You can adjust batch sizes in each file if needed:
- Smaller batch size = smoother but slower initialization
- Larger batch size = faster but more noticeable micro-stutters

Current batch sizes are conservative for smooth experience.
