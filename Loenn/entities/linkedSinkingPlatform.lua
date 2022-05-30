local resortPlatformHelper = require("helpers.resort_platforms")
local utils = require("utils")

local textures = {
    "default", "cliffside"
}
local textureOptions = {}

for _, texture in ipairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

local sinkingPlatform = {}

sinkingPlatform.name = "AidenHelper/LinkedSinkingPlatform"
sinkingPlatform.depth = 1
sinkingPlatform.fieldInformation = {
    texture = {
        options = textureOptions
    }
}
sinkingPlatform.placements = {}

for i, texture in ipairs(textures) do
    sinkingPlatform.placements[i] = {
        name = texture,
        data = {
            width = 16,
            texture = texture
        }
    }
end

function sinkingPlatform.sprite(room, entity)
    local sprites = {}

    -- Prevent visual oddities with too long lines
    local x, y = entity.x or 0, entity.y or 0
    local nodeY = room.height - 2

    if y > nodeY then
        nodeY = y
    end

    resortPlatformHelper.addConnectorSprites(sprites, entity, x, y, x, nodeY)
    resortPlatformHelper.addPlatformSprites(sprites, entity, entity)

    return sprites
end

sinkingPlatform.selection = resortPlatformHelper.getSelection

return sinkingPlatform