local fakeTilesHelper = require("helpers.fake_tiles")

local dashDashBlock = {}

dashDashBlock.name = "AidenHelper/DashDashBlock"
dashDashBlock.depth = 0
dashDashBlock.placements = {
    name = "Dash Dash Block",
    data = {
        tiletype = "3",
        blendin = false,
        permanent = false,
        horizontal = true,
        vertical = true,
        width = 8,
        height = 8
    }
}

dashDashBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")
dashDashBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return dashDashBlock