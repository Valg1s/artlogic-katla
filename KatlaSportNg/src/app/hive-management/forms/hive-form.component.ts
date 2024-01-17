import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HiveService } from '../services/hive.service';
import { Hive } from '../models/hive';

@Component({
  selector: 'app-hive-form',
  templateUrl: './hive-form.component.html',
  styleUrls: ['./hive-form.component.css']
})
export class HiveFormComponent implements OnInit {

  hive = new Hive(0, "", "", "", false, "");
  existed = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private hiveService: HiveService
  ) { }

  ngOnInit() {
    this.route.params.subscribe(p => {
      if (p['id'] === undefined) return;
      this.hiveService.getHive(p['id']).subscribe(h => this.hive = h);
      this.existed = true;
    });
  }

  navigateToHives() {
    this.router.navigate(['/hives']);
  }

  onCancel() {
    this.navigateToHives();
  }
  
  onSubmit() {
    var answer;
    
    if (this.existed) {
      answer = this.hiveService.updateHive(this.hive);
    }
    else{
      answer = this.hiveService.addHive(this.hive);
    }
    
    answer.subscribe(() => this.navigateToHives());
  }

  onDelete() {
    this.hiveService.setHiveStatus(this.hive.id, true).subscribe(c => this.hive.isDeleted = true);
  }

  onUndelete() {
    this.hiveService.setHiveStatus(this.hive.id, false).subscribe(c => this.hive.isDeleted = false);
  }

  onPurge() {
    if (this.hive.isDeleted){
      this.hiveService.deleteHive(this.hive.id).subscribe(() => this.navigateToHives());
    }
  }
}
