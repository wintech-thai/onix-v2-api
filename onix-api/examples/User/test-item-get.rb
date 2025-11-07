#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

### AddItem
apiGetItemsUrl = "api/Item/org/#{orgId}/action/GetItems"
apiGetItemByIdUrl = "api/Item/org/#{orgId}/action/GetItemById"

param =  {
  FullTextSearch: ""
}

result = make_request(:post, apiGetItemsUrl, param)

result.each do |item|
  puts("ID: [#{item['id']}]")
  itemId = item['id']

  item = make_request(:get, "#{apiGetItemByIdUrl}/#{itemId}", nil)
  puts(item)
end
