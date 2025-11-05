#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']

### AddItem
apiUrl = "api/Privilege/org/#{orgId}/action/AddPrivilege"
item =  {
  Code: "LUX-SPA-NOVOTEL-002",
  Description: "This is product description # 2",
  Tags: "ทดสอบ,dddd,bbb,food,feed",
  Content: "This is very long content",
}

result = make_request(:post, apiUrl, item)
puts(result)
