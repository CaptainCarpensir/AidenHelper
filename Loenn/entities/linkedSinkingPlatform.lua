local resortPlatformHelper = require("helpers.resort_platforms")
local utils = require("utils")

local textures = {
    "default", "cliffside"
}
local textureOptions = {}

for _, texture in ipairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

local linkedSinkingPlatform = {}

linkedSinkingPlatform.name = "AidenHelper/LinkedSinkingPlatform"
linkedSinkingPlatform.depth = 1
linkedSinkingPlatform.nodeLimits = {1,1}
linkedSinkingPlatform.fieldInformation = {
    texture = {
        options = textureOptions
    }
}

linkedSinkingPlatform.placements = {
    name = "linkedSinkingPlatform",
}

for i, texture in ipairs(textures) do
    linkedSinkingPlatform.placements[i] = {
        name = texture,
        data = {
            width = 16,
            texture = texture
        }
    }
end

function linkedSinkingPlatform.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {{x = x, y = y+16}}
    local nodeY = nodes[1].y

    resortPlatformHelper.addConnectorSprites(sprites, entity, x, y, x, nodeY)
    resortPlatformHelper.addPlatformSprites(sprites, entity, entity)

    return sprites
end

function linkedSinkingPlatform.nodeSprite(room, entity, node)
    local normalizedNode = node
    normalizedNode.x = entity.x

    return resortPlatformHelper.addPlatformSprites({}, entity, normalizedNode)
end

linkedSinkingPlatform.selection = resortPlatformHelper.getSelection

return linkedSinkingPlatform