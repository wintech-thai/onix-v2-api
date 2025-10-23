#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']

param =  {
  UserName: "pjamenaja7",
  TmpUserEmail: "pjame.fb@gmail.com",
  Roles: [ "OWNER" ],
}

### Inviteuser
apiUrl = "api/OrganizationUser/org/#{orgId}/action/Inviteuser"
result = make_request(:post, apiUrl, param)
puts(result)
