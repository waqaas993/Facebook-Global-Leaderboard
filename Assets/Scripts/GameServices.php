<?php

//db connectivity code here
$hostname = "xxx";
$username = "xxx";
$password = "xxx";
$dbname   = "xxx";

$conn = mysqli_connect($hostname, $username, $password, $dbname) OR DIE ("Unable to connect to database! Please try again later.");

//housekeepign variables
$action = $_REQUEST['action'];
$gameId = $_REQUEST['gameId'];          
$response = array();

//switch case to consolidate the db interaction
$fbId = $_REQUEST['fbId'];          
$name = $_REQUEST['name'];          
$email = $_REQUEST['email'];    
$score = $_REQUEST['Score'];        
$limit = (isset($_REQUEST['limit']))?$_REQUEST['limit']:10;
$limit = ($limit > 100)?100:$limit;

switch ($gameId)
{
    case 'com.waqasdevs.globallb': 
        $table = 'fb_GlobalLeaderboard';
		$loginQuery = "INSERT INTO $table (id, name, email, score) VALUES ('$fbId', '$name', '$email', '0') ON DUPLICATE KEY UPDATE name=VALUES(name), email=VALUES(email)";
		$updateQuery = "UPDATE $table SET score='$score' WHERE id='$fbId'";
		$scoreQuery = "SELECT score FROM $table WHERE id='$fbId'";
	break;

	case 'com.waqasdevs.weeklylb': 

	    $time = date('Y-m-d h:i:s');
	    $date = new DateTime($time);
	    $week = $date->format("W");
	    $year = $date->format("Y");

	    $table = 'fb_WeeklyLeaderboard'.$week.'_'.$year;

	    $query = "CREATE TABLE if NOT EXISTS $table (
        id INT(11) UNSIGNED AUTO_INCREMENT PRIMARY KEY, 
        name VARCHAR(50) NOT NULL DEFAULT '',
        score VARCHAR(50) NOT NULL DEFAULT '0',
        INDEX(score)
        )";

        if (mysqli_query($conn, $query)) {
        	return true;
        }
        else {
        	echo "Error creating table: " . mysqli_error($conn);
        }

        mysqli_close($conn);

		$loginQuery = "INSERT INTO $table (id, name, score) VALUES ('$fbId', '$name', '0') ON DUPLICATE KEY UPDATE name=VALUES(name)";
		$updateQuery = "UPDATE $table SET score = score + '$score' WHERE id='$fbId'";
		$scoreQuery = "SELECT score FROM $table WHERE id='$fbId'";

	break;

	default:
		die(json_encode("Game ID not found!!!"));
	break;

}

//switch case to consolidate the web interaction
switch ($action)
{
	case 'login':
		$response = array();

		$status = mysqli_query($conn, $loginQuery);

		$row = mysqli_fetch_array(mysqli_query($conn, $scoreQuery),MYSQLI_ASSOC);

		$response = $row;
		$response['status'] = $status;

		echo json_encode($response);
	break;

	case 'updateScore':

		if(mysqli_query($conn, $updateQuery))
			$response['status'] = true;
		else
			$response['status'] = false;

		echo json_encode($response);
	break;

	case 'getScore':

		$response = array();

		$globalLBQuery = ($globalLBQuery == '')?"SELECT * FROM $table WHERE name <> '' ORDER BY score DESC LIMIT 0,$limit":$globalLBQuery;
		$result = mysqli_query($conn, $globalLBQuery);
		while ($row = mysqli_fetch_assoc($result))
			$response[] = $row;

		$response['status'] = true;

		echo json_encode($response);
	break;

	default:
		$response['msg'] = 'wat!';
		$response['status'] = false;

		echo json_encode($response);
		break;
}
?>