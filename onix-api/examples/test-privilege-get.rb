#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']

apiGetItemsUrl = "api/Privilege/org/#{orgId}/action/GetPrivileges"
apiGetItemCountUrl = "api/Privilege/org/#{orgId}/action/GetPrivilegeCount"
apiGetItemByIdUrl = "api/Privilege/org/#{orgId}/action/GetPrivilegeById"

param =  {
  FullTextSearch: ""
}

result = make_request(:post, apiGetItemsUrl, param)

cnt = make_request(:post, apiGetItemCountUrl, param)
puts(cnt)

result.each do |item|
  puts("ID: [#{item['id']}]")
  itemId = item['id']

  item = make_request(:get, "#{apiGetItemByIdUrl}/#{itemId}", nil)
  puts(item)
end
