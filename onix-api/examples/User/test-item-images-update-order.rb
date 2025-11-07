#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = "napbiotec"
itemId = "99b5dbd5-7a79-4560-9a89-8c24b0393229"

### DeleteItemImagesByItemId
apiItemImagesUpdateOrderUrl = "api/Item/org/#{orgId}/action/UpdateItemImagesSortingOrder/#{itemId}"
idList = [
  '6251b743-e659-4667-838f-2ffd7d96be83',
  'c08587c9-0c09-467c-8907-2fdbd9f9151a',
  '5741cd3d-b03a-4df6-b623-0ddfc223027a',
  '9c6ae9a0-7b2f-4dde-9bb7-c4455c61f705',
]
result = make_request(:post, apiItemImagesUpdateOrderUrl, idList)
puts(result)
