local MagnetToggle = {}

MagnetToggle.name = "Aqua/Magnet Toggle"
MagnetToggle.depth = 2000
MagnetToggle.justification = { 0.5, 0.5, }
MagnetToggle.placements = {
    name = "Magnet Toggle",
    data = {
        flag = "Magnet1",
        one_use = false,
        holdable_can_activate = false,
    }
}

MagnetToggle.texture = "objects/hook_magnet/troggle00"

return MagnetToggle